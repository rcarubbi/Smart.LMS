using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;
using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.Application.Assessments;

public sealed record OptionInput(string Text, int Order);

public sealed record QuestionInput(
    string Type,
    string Description,
    int Score,
    IReadOnlyList<OptionInput> Options,
    int? SingleChoiceRightOrder,
    IReadOnlyList<int> MultipleChoiceRightOrders);

public sealed record AssessmentDetail(
    Guid Id,
    Guid CourseId,
    int MinimumScore,
    IReadOnlyList<QuestionDetail> Questions);

public sealed record QuestionDetail(
    Guid Id,
    string Type,
    string Description,
    int Score,
    IReadOnlyList<OptionDetail> Options);

public sealed record OptionDetail(Guid Id, string Text, int Order);

// ----- Commands -----

public sealed record CreateAssessmentCommand(
    Guid CourseId,
    int MinimumScore,
    IReadOnlyList<QuestionInput> Questions) : ICommand<Guid>;

internal sealed class CreateAssessmentValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentValidator()
    {
        RuleFor(c => c.CourseId).NotEmpty();
        RuleFor(c => c.MinimumScore).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Questions).NotEmpty();

        RuleForEach(c => c.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.Description).NotEmpty();
            q.RuleFor(x => x.Score).GreaterThan(0);
            q.RuleFor(x => x.Options).NotEmpty();
            q.RuleFor(x => x.Type).Must(t => Enum.TryParse<QuestionTypes>(t, ignoreCase: true, out _))
                .WithMessage("Unknown question type.");
        });
    }
}

internal sealed class CreateAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<CreateAssessmentCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == command.CourseId, cancellationToken);
        if (!courseExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Courses.NotFound", "Course not found."));
        }

        var assessment = new Assessment
        {
            CourseId = command.CourseId,
            MinimumScore = command.MinimumScore,
            Active = true,
        };

        var pendingRightAnswers = new List<(SingleChoiceQuestion? Single, int? RightOrder, MultipleChoiceQuestion? Multiple, IReadOnlyList<int> RightOrders)>();

        foreach (var questionInput in command.Questions)
        {
            var type = Enum.Parse<QuestionTypes>(questionInput.Type, ignoreCase: true);

            Question question = type switch
            {
                QuestionTypes.SingleChoice => new SingleChoiceQuestion(),
                QuestionTypes.MultipleChoice => new MultipleChoiceQuestion(),
                QuestionTypes.Reordering => new ReorderingQuestion(),
                _ => throw new InvalidOperationException($"Unknown question type '{questionInput.Type}'."),
            };

            question.Description = questionInput.Description;
            question.Score = questionInput.Score;
            question.Assessment = assessment;

            var options = questionInput.Options
                .OrderBy(o => o.Order)
                .Select(o => new QuestionOption { Text = o.Text, Order = o.Order, Question = question })
                .ToList();

            foreach (var option in options)
            {
                question.Options.Add(option);
            }

            assessment.Questions.Add(question);
            pendingRightAnswers.Add((
                question as SingleChoiceQuestion,
                questionInput.SingleChoiceRightOrder,
                question as MultipleChoiceQuestion,
                questionInput.MultipleChoiceRightOrders ?? []));
        }

        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(cancellationToken);

        // Second pass wires right answers once options exist in the database
        // (question -> option -> question would otherwise be an unsavable cycle).
        foreach (var (single, rightOrder, multiple, rightOrders) in pendingRightAnswers)
        {
            if (single is { } s)
            {
                if (rightOrder is { } order)
                {
                    var rightOption = s.Options.ElementAtOrDefault(order)
                        ?? throw new InvalidOperationException("Single choice right answer index is out of range.");
                    s.RightOptionId = rightOption.Id;
                }
            }
            else if (multiple is { } m)
            {
                foreach (var index in rightOrders)
                {
                    var rightOption = m.Options.ElementAtOrDefault(index)
                        ?? throw new InvalidOperationException("Multiple choice right answer indexes are out of range.");
                    m.RightOptions.Add(rightOption);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(assessment.Id);
    }
}

public sealed record DeleteAssessmentCommand(Guid Id) : ICommand;

internal sealed class DeleteAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteAssessmentCommand>
{
    public async ValueTask<Result> Handle(DeleteAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments.FindAsync([command.Id], cancellationToken);
        if (assessment is null)
        {
            return Result.Failure(Error.NotFound("Assessments.NotFound", "Assessment not found."));
        }

        context.Assessments.Remove(assessment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// ----- Queries -----

public sealed record ListAssessmentsQuery(Guid CourseId) : IQuery<IReadOnlyList<AssessmentListItem>>;

public sealed record AssessmentListItem(Guid Id, string CourseName, int MinimumScore, int QuestionCount);

internal sealed class ListAssessmentsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListAssessmentsQuery, IReadOnlyList<AssessmentListItem>>
{
    public async ValueTask<Result<IReadOnlyList<AssessmentListItem>>> Handle(
        ListAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Assessments
            .Where(a => a.Active && a.CourseId == query.CourseId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AssessmentListItem(a.Id, a.Course.Name, a.MinimumScore, a.Questions.Count))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AssessmentListItem>>(items);
    }
}
