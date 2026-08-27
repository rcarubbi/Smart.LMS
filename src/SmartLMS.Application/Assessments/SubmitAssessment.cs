using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.Assessments;

namespace SmartLMS.Application.Assessments;

// ----- Submission -----

public sealed record QuestionSubmission(Guid QuestionId, IReadOnlyList<Guid> OptionIds);

public sealed record SubmitAssessmentCommand(
    Guid AssessmentId,
    IReadOnlyList<QuestionSubmission> Questions) : ICommand<AssessmentResult>;

public sealed record QuestionResult(Guid QuestionId, bool Correct, int ScoreAwarded);

public sealed record AssessmentResult(
    Guid AssessmentId,
    string CourseName,
    int Score,
    int MinimumScore,
    bool Passed,
    IReadOnlyList<QuestionResult> Questions);

/// <summary>
/// Pure scoring rules: single choice compares the picked option; multiple choice
/// requires exact set equality; reordering requires the submitted option sequence to
/// match the options ordered by <see cref="QuestionOption.Order"/>.
/// </summary>
public static class AssessmentScorer
{
    public static (int Score, List<QuestionResult> PerQuestion) Score(
        IEnumerable<(Question Question, QuestionSubmission? Submission)> pairs)
    {
        int total = 0;
        List<QuestionResult> results = [];

        foreach (var (question, submission) in pairs)
        {
            var submitted = submission?.OptionIds ?? [];
            var correct = IsCorrect(question, submitted);

            if (correct)
            {
                total += question.Score;
            }

            results.Add(new QuestionResult(question.Id, correct, correct ? question.Score : 0));
        }

        return (total, results);
    }

    public static bool IsCorrect(Question question, IReadOnlyList<Guid> submitted)
    {
        return question switch
        {
            SingleChoiceQuestion single => submitted.Count == 1 && submitted[0] == single.RightOptionId,
            MultipleChoiceQuestion multiple =>
                submitted.Count == multiple.RightOptions.Count &&
                submitted.All(id => multiple.RightOptions.Any(r => r.Id == id)),

            ReorderingQuestion =>
                submitted.SequenceEqual(
                    [.. question.Options.OrderBy(o => o.Order).Select(o => o.Id)]),

            _ => false,
        };
    }
}

internal sealed class SubmitAssessmentHandler(ISmartLmsDbContext context) : ICommandHandler<SubmitAssessmentCommand, AssessmentResult>
{
    public async ValueTask<ErrorOr<AssessmentResult>> Handle(SubmitAssessmentCommand command, CancellationToken cancellationToken)
    {
        var assessment = await context.Assessments
            .Include(a => a.Course)
            .Include(a => a.Questions)
                .ThenInclude(q => q.Options)
            .SingleOrDefaultAsync(a => a.Id == command.AssessmentId, cancellationToken);

        if (assessment is null || !assessment.Active)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        // TPH-specific navigations (RightOption/RightOptions) are fixed up automatically:
        // their targets are among the already-tracked QuestionOption instances.
        var submissionsByQuestion = command.Questions.ToDictionary(s => s.QuestionId);
        var pairs = assessment.Questions.Select(q =>
            (q, submissionsByQuestion.GetValueOrDefault(q.Id)));

        var (score, perQuestion) = AssessmentScorer.Score(pairs);

        return new AssessmentResult(
            assessment.Id,
            assessment.Course.Name,
            score,
            assessment.MinimumScore,
            score >= assessment.MinimumScore,
            perQuestion);
    }
}

public sealed record GetAssessmentQuery(Guid Id) : IQuery<AssessmentDetail>;

/// <summary>Student-facing view: questions and options only, never the right answers.</summary>
internal sealed class GetAssessmentHandler(ISmartLmsDbContext context) : IQueryHandler<GetAssessmentQuery, AssessmentDetail>
{
    public async ValueTask<ErrorOr<AssessmentDetail>> Handle(GetAssessmentQuery query, CancellationToken cancellationToken)
    {
        var detail = await context.Assessments
            .Where(a => a.Active && a.Id == query.Id)
            .Select(a => new AssessmentDetail(
                a.Id,
                a.CourseId,
                a.MinimumScore,
                a.Questions.OrderBy(q => q.CreatedAt)
                    .Select(q => new QuestionDetail(
                        q.Id,
                        EF.Property<string>(q, "Discriminator"),
                        q.Description,
                        q.Score,
                        q.Options.OrderBy(o => o.Order)
                            .Select(o => new OptionDetail(o.Id, o.Text, o.Order))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null
            ? Error.NotFound("Assessments.NotFound", "Assessment not found.")
            : detail;
    }
}
