using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Authorization;
using ErrorOr;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Assessments;

public sealed record OptionInput(string Text, int Order);

public sealed record QuestionInput(
    string Type,
    string Description,
    int Score,
    IReadOnlyList<OptionInput> Options,
    int? SingleChoiceRightOrder,
    IReadOnlyList<int> MultipleChoiceRightOrders);

/// <summary>A question built by the factory plus its right-answer wiring, pending until
/// option rows exist in the database (question -> option -> question is an unsavable cycle).</summary>
internal sealed record PendingQuestion(
    Question Question,
    int? SingleChoiceRightOrder,
    IReadOnlyList<int> MultipleRightOrders);

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

public sealed record OptionDetail(Guid Id, string Text);

// ----- Commands -----

public sealed record CreateAssessmentInput(
    Guid CourseId,
    int MinimumScore,
    IReadOnlyList<QuestionInput> Questions);

public sealed record CreateAssessmentCommand(CreateAssessmentInput Input) : ICommand<Guid>;

internal sealed class CreateAssessmentValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentValidator()
    {
        RuleFor(c => c.Input.CourseId).NotEmpty();
        RuleFor(c => c.Input.MinimumScore).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.Questions).NotEmpty();

        RuleForEach(c => c.Input.Questions).SetValidator(new QuestionInputValidator());
    }
}

internal sealed class CreateAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<CreateAssessmentCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == command.Input.CourseId, cancellationToken);
        if (!courseExists)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var assessment = new Assessment(command.Input.CourseId, command.Input.MinimumScore);
        var pending = AssessmentQuestionFactory.BuildAll(assessment, command.Input.Questions);

        context.Assessments.Add(assessment);
        await context.SaveChangesAsync(cancellationToken);

        // Second pass wires right answers once options exist in the database.
        var wire = AssessmentQuestionFactory.WireRightAnswers(pending);
        if (wire.IsError)
        {
            return wire.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}

/// <summary>Shared per-question rules (including right-answer index ranges) for
/// create/update: fail fast with 400 before any transaction opens.</summary>
internal sealed class QuestionInputValidator : AbstractValidator<QuestionInput>
{
    public QuestionInputValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Score).GreaterThan(0);
        RuleFor(x => x.Options).NotEmpty();
        RuleFor(x => x.Type).Must(t => Enum.TryParse<QuestionTypes>(t, ignoreCase: true, out _))
            .WithMessage("Unknown question type.");
        RuleFor(x => x.SingleChoiceRightOrder)
            .Must((qi, order) => order is null || (order >= 0 && order < qi.Options.Count))
            .WithMessage("Single choice right answer index is out of range.");
        RuleForEach(x => x.MultipleChoiceRightOrders)
            .Must((qi, index) => index >= 0 && index < qi.Options.Count)
            .WithMessage("Multiple choice right answer index is out of range.");
    }
}

/// <summary>
/// Shared question construction + right-answer wiring for create/update.
/// Building and wiring stay in one place so both commands share semantics.
/// </summary>
internal static class AssessmentQuestionFactory
{
    /// <summary>Registry of question-type constructors (strategy table): adding a question
    /// type means adding its class plus one entry here, not editing a dispatch switch.</summary>
    private static readonly IReadOnlyDictionary<QuestionTypes, Func<string, int, Question>> Factories =
        new Dictionary<QuestionTypes, Func<string, int, Question>>
        {
            [QuestionTypes.SingleChoice] = static (description, score) => new SingleChoiceQuestion(description, score),
            [QuestionTypes.MultipleChoice] = static (description, score) => new MultipleChoiceQuestion(description, score),
            [QuestionTypes.Reordering] = static (description, score) => new ReorderingQuestion(description, score),
        };

    public static Question Create(QuestionInput input)
    {
        var type = Enum.Parse<QuestionTypes>(input.Type, ignoreCase: true);
        if (!Factories.TryGetValue(type, out var factory))
        {
            throw new InvalidOperationException($"Unknown question type '{input.Type}'.");
        }

        var question = factory(input.Description, input.Score);

        foreach (var optionInput in input.Options.OrderBy(o => o.Order))
        {
            question.AddOption(optionInput.Text, optionInput.Order);
        }

        return question;
    }

    /// <summary>Builds all questions, attaches them to the assessment and records
    /// the wiring pending until option rows exist in the database.</summary>
    public static IReadOnlyList<PendingQuestion> BuildAll(Assessment assessment, IReadOnlyList<QuestionInput> inputs)
    {
        var pending = new List<PendingQuestion>(inputs.Count);
        foreach (var input in inputs)
        {
            var question = Create(input);
            question.AttachAssessment(assessment);
            assessment.AddQuestion(question);
            pending.Add(new PendingQuestion(question, input.SingleChoiceRightOrder, input.MultipleChoiceRightOrders ?? []));
        }

        return pending;
    }

    public static ErrorOr<Success> WireRightAnswers(IReadOnlyList<PendingQuestion> pending)
    {
        foreach (var p in pending)
        {
            if (!p.Question.TryWireRightAnswers(p.SingleChoiceRightOrder, p.MultipleRightOrders, out var error))
            {
                return Error.Validation("Assessments.RightAnswerOutOfRange", error ?? "Right answer index is out of range.");
            }
        }

        return Result.Success;
    }
}

public sealed record UpdateAssessmentInput(
    int MinimumScore,
    IReadOnlyList<QuestionInput> Questions);

public sealed record UpdateAssessmentCommand(Guid Id, UpdateAssessmentInput Input) : ICommand;

internal sealed class UpdateAssessmentValidator : AbstractValidator<UpdateAssessmentCommand>
{
    public UpdateAssessmentValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Input.MinimumScore).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.Questions).NotEmpty();

        RuleForEach(c => c.Input.Questions).SetValidator(new QuestionInputValidator());
    }
}

internal sealed class UpdateAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateAssessmentCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var spec = new AssessmentWithQuestionsSpec(command.Id);
        var assessment = await SpecificationEvaluator.Default
            .GetQuery(context.Assessments, spec)
            .SingleOrDefaultAsync(cancellationToken);
        if (assessment is null)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        return await AssessmentReplacer.ReplaceAsync(
            context,
            command.Id,
            assessment,
            command.Input.MinimumScore,
            command.Input.Questions,
            cancellationToken);
    }
}

/// <summary>EF choreography for atomically swapping an assessment's questions. Flush
/// ordering matters: right-answer FKs point at options with NoAction delete behavior, so
/// references must be cleared before rows are deleted, and Deleted + Added entries for the
/// same graph must not share one SaveChanges (stale UPDATEs for deleted keys, dropped
/// inserts). One concern per flush; the decorator keeps the whole replace in one transaction.</summary>
internal static class AssessmentReplacer
{
    public static async ValueTask<ErrorOr<Success>> ReplaceAsync(
        ISmartLmsDbContext context,
        Guid assessmentId,
        Assessment assessment,
        int minimumScore,
        IReadOnlyList<QuestionInput> inputs,
        CancellationToken cancellationToken)
    {
        // Load right-answer navigations with typed queries (no fragile string includes):
        // same DbContext returns the same tracked instances as assessment.Questions.
        var singles = await context.Questions.OfType<SingleChoiceQuestion>()
            .Where(q => q.AssessmentId == assessmentId)
            .ToListAsync(cancellationToken);
        var multiples = await context.Questions.OfType<MultipleChoiceQuestion>()
            .Where(q => q.AssessmentId == assessmentId)
            .Include(m => m.RightOptions)
            .ToListAsync(cancellationToken);

        // Clear FK references first, flushed alone: nulling + deleting rows in a single
        // SaveChanges is a circular dependency EF cannot order (Q -RightOptionId-> O -QuestionId-> Q).
        foreach (var single in singles)
        {
            single.ClearRightOption();
        }

        foreach (var multiple in multiples)
        {
            multiple.ClearRightOptions();
        }

        await context.SaveChangesAsync(cancellationToken);

        // Deletes flush alone: mixing Deleted old rows and Added new rows for the same
        // graph in one SaveChanges corrupts the update pipeline.
        var oldQuestions = assessment.Questions.ToList();
        if (oldQuestions.Count > 0)
        {
            context.Questions.RemoveRange(oldQuestions);
        }

        await context.SaveChangesAsync(cancellationToken);

        assessment.ClearQuestions();
        assessment.SetMinimumScore(minimumScore);
        var pending = AssessmentQuestionFactory.BuildAll(assessment, inputs);

        // Explicit add (mirrors create): no Deleted entries coexist anymore,
        // so this flush is a plain assessment UPDATE plus clean INSERTs.
        context.Questions.AddRange(pending.Select(p => p.Question));

        // Inserts flush together; wiring follows once option rows exist.
        await context.SaveChangesAsync(cancellationToken);

        var wire = AssessmentQuestionFactory.WireRightAnswers(pending);
        if (wire.IsError)
        {
            return wire.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record OptionEditDetail(Guid Id, string Text, int Order);

public sealed record QuestionEditDetail(
    Guid Id,
    string Type,
    string Description,
    int Score,
    IReadOnlyList<OptionEditDetail> Options,
    int? SingleChoiceRightOrder,
    IReadOnlyList<int> MultipleChoiceRightOrders);

public sealed record AssessmentEditDetail(
    Guid Id,
    Guid CourseId,
    int MinimumScore,
    IReadOnlyList<QuestionEditDetail> Questions);

public sealed record GetAssessmentForEditQuery(Guid Id) : IQuery<AssessmentEditDetail>;

/// <summary>Admin-only view WITH right answers for the assessment editor.</summary>
internal sealed class GetAssessmentForEditHandler(ISmartLmsDbContext context)
    : IQueryHandler<GetAssessmentForEditQuery, AssessmentEditDetail>
{
    public async ValueTask<ErrorOr<AssessmentEditDetail>> Handle(
        GetAssessmentForEditQuery query,
        CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments.AsNoTracking()
            .Where(a => a.Id == query.Id)
            .Select(a => new { a.Id, a.CourseId, a.MinimumScore })
            .SingleOrDefaultAsync(cancellationToken);
        if (assessment is null)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        // Tracking on purpose: small admin-only graph, typed includes, no shadow-FK tricks.
        var questions = await context.Questions
            .Where(q => q.AssessmentId == query.Id)
            .Include(q => q.Options)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
        var singles = await context.Questions.OfType<SingleChoiceQuestion>()
            .Where(q => q.AssessmentId == query.Id)
            .ToListAsync(cancellationToken);
        var multiples = await context.Questions.OfType<MultipleChoiceQuestion>()
            .Where(q => q.AssessmentId == query.Id)
            .Include(m => m.RightOptions)
            .ToListAsync(cancellationToken);

        var singleById = singles.ToDictionary(s => s.Id);
        var multipleById = multiples.ToDictionary(m => m.Id);

        List<QuestionEditDetail> items = [];
        foreach (var question in questions)
        {
            var ordered = question.Options.OrderBy(o => o.Order).ToList();
            var indexByOptionId = ordered
                .Select((o, i) => (o.Id, Index: i))
                .ToDictionary(x => x.Id, x => x.Index);

            int? singleRight = null;
            IReadOnlyList<int> multipleRights = [];

            if (singleById.TryGetValue(question.Id, out var single) && single.RightOptionId.HasValue
                && indexByOptionId.TryGetValue(single.RightOptionId.Value, out var rightIndex))
            {
                singleRight = rightIndex;
            }
            else if (multipleById.TryGetValue(question.Id, out var multiple))
            {
                multipleRights = [.. multiple.RightOptions
                    .Where(r => indexByOptionId.ContainsKey(r.Id))
                    .Select(r => indexByOptionId[r.Id])
                    .Order()];
            }

            items.Add(new QuestionEditDetail(
                question.Id,
                question.Type.ToString(),
                question.Description,
                question.Score,
                [.. ordered.Select(o => new OptionEditDetail(o.Id, o.Text, o.Order))],
                singleRight,
                multipleRights));
        }

        return new AssessmentEditDetail(assessment.Id, assessment.CourseId, assessment.MinimumScore, items);
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

        assessment.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreAssessmentCommand(Guid Id) : ICommand;

internal sealed class RestoreAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreAssessmentCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments
            .Include(a => a.Course)
            .SingleOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (assessment is null)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        if (!assessment.Course.Active)
        {
            return Error.Conflict(
                "Assessments.CourseInactive",
                "O curso da avaliação está na lixeira. Restaure o curso antes de restaurar a avaliação.");
        }

        assessment.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteAssessmentPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteAssessmentPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteAssessmentPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(
        DeleteAssessmentPermanentlyCommand command,
        CancellationToken cancellationToken)
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

public sealed record ListAssessmentsQuery(
    Guid CourseId,
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<AssessmentListItem>>;

public sealed record AssessmentListItem(Guid Id, string CourseName, int MinimumScore, int QuestionCount, bool Active);

internal sealed class ListAssessmentsHandler(ISmartLmsDbContext context, ClassAccessPolicy accessPolicy)
    : IQueryHandler<ListAssessmentsQuery, Paged<AssessmentListItem>>
{
    public async ValueTask<ErrorOr<Paged<AssessmentListItem>>> Handle(
        ListAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        if (!await accessPolicy.CanAccessCourseAsync(query.CourseId, cancellationToken))
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var spec = new AssessmentsByCourseSpec(query.CourseId, query.Active);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.Assessments.AsNoTracking(), spec);

        var items = await filtered
            .Select(a => new AssessmentListItem(a.Id, a.Course.Name, a.MinimumScore, a.Questions.Count, a.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
