using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
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
    public async ValueTask<ErrorOr<Guid>> Handle(CreateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == command.CourseId, cancellationToken);
        if (!courseExists)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var assessment = new Assessment(command.CourseId, command.MinimumScore);

        var pendingRightAnswers = new List<(SingleChoiceQuestion? Single, int? RightOrder, MultipleChoiceQuestion? Multiple, IReadOnlyList<int> RightOrders)>();

        foreach (var questionInput in command.Questions)
        {
            var type = Enum.Parse<QuestionTypes>(questionInput.Type, ignoreCase: true);

            Question question = type switch
            {
                QuestionTypes.SingleChoice => new SingleChoiceQuestion(questionInput.Description, questionInput.Score),
                QuestionTypes.MultipleChoice => new MultipleChoiceQuestion(questionInput.Description, questionInput.Score),
                QuestionTypes.Reordering => new ReorderingQuestion(questionInput.Description, questionInput.Score),
                _ => throw new InvalidOperationException($"Unknown question type '{questionInput.Type}'."),
            };

            question.AttachAssessment(assessment);

            foreach (var optionInput in questionInput.Options.OrderBy(o => o.Order))
            {
                question.AddOption(optionInput.Text, optionInput.Order);
            }

            assessment.AddQuestion(question);
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
                    s.SetRightOption(rightOption.Id);
                }
            }
            else if (multiple is { } m)
            {
                foreach (var index in rightOrders)
                {
                    var rightOption = m.Options.ElementAtOrDefault(index)
                        ?? throw new InvalidOperationException("Multiple choice right answer indexes are out of range.");
                    m.AddRightOption(rightOption);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}

public sealed record DeleteAssessmentCommand(Guid Id) : ICommand;

internal sealed class DeleteAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteAssessmentCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments.FindAsync([command.Id], cancellationToken);
        if (assessment is null)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        context.Assessments.Remove(assessment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

// ----- Queries -----

public sealed record ListAssessmentsQuery(Guid CourseId) : IQuery<IReadOnlyList<AssessmentListItem>>;

public sealed record AssessmentListItem(Guid Id, string CourseName, int MinimumScore, int QuestionCount);

internal sealed class ListAssessmentsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListAssessmentsQuery, IReadOnlyList<AssessmentListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<AssessmentListItem>>> Handle(
        ListAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Assessments
            .Where(a => a.Active && a.CourseId == query.CourseId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AssessmentListItem(a.Id, a.Course.Name, a.MinimumScore, a.Questions.Count))
            .ToListAsync(cancellationToken);

        return items;
    }
}
