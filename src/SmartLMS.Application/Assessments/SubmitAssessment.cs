using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Assessments;

// ----- Submission -----

public sealed record QuestionSubmission(Guid QuestionId, IReadOnlyList<Guid> OptionIds);

public sealed record SubmitAssessmentInput(IReadOnlyList<QuestionSubmission> Questions);

public sealed record SubmitAssessmentCommand(
    Guid AssessmentId,
    SubmitAssessmentInput Input) : ICommand<AssessmentResult>;

public sealed record QuestionResult(Guid QuestionId, bool Correct, int ScoreAwarded);

public sealed record AssessmentResult(
    Guid AssessmentId,
    string CourseName,
    int Score,
    int MinimumScore,
    bool Passed,
    IReadOnlyList<QuestionResult> Questions);

internal sealed class SubmitAssessmentHandler(
        ISmartLmsDbContext context,
        ICurrentUser currentUser)
    : ICommandHandler<SubmitAssessmentCommand, AssessmentResult>
{
    public async ValueTask<ErrorOr<AssessmentResult>> Handle(SubmitAssessmentCommand command, CancellationToken cancellationToken)
    {
        // Tracking on purpose: TPH fixup of RightOption navigations
        // relies on the already-tracked QuestionOption instances.
        var assessmentSpec = new AssessmentWithQuestionsSpec(command.AssessmentId);
        var assessment = await SpecificationEvaluator.Default
            .GetQuery(context.Assessments, assessmentSpec)
            .SingleOrDefaultAsync(cancellationToken);

        if (assessment is null || !assessment.Active)
        {
            return Error.NotFound("Assessments.NotFound", "Assessment not found.");
        }

        // Only admins are exempt: everyone else must finish every class of the course first.
        if (currentUser.Role != UserRole.Admin && currentUser.UserId is { } userId)
        {
            var classIds = await SpecificationEvaluator.Default
                .GetQuery(context.Classes.AsNoTracking(), new ClassesByCourseSpec(assessment.CourseId))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            if (classIds.Count > 0)
            {
                var completedIds = await SpecificationEvaluator.Default
                    .GetQuery(context.ClassAccesses.AsNoTracking(), new CompletedClassAccessesSpec(userId, classIds))
                    .Select(a => a.ClassId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var missing = classIds.Except(completedIds).ToList();
                if (missing.Count > 0)
                {
                    return Error.Forbidden(
                        "Assessments.ClassesNotCompleted",
                        "Conclua todas as aulas do curso antes de fazer a avaliação.");
                }
            }
        }

        // Scoring lives on the aggregate; the handler only adapts input/output DTOs
        // and enriches with CourseName (read-model concern, not a domain rule).
        var answers = command.Input.Questions.ToDictionary(s => s.QuestionId, s => s.OptionIds);
        var scored = assessment.Score(answers);

        return new AssessmentResult(
            assessment.Id,
            assessment.Course.Name,
            scored.Total,
            assessment.MinimumScore,
            scored.Passed,
            [.. scored.PerQuestion.Select(q => new QuestionResult(q.QuestionId, q.Correct, q.Awarded))]);
    }
}

public sealed record GetAssessmentQuery(Guid Id) : IQuery<AssessmentDetail>;

/// <summary>Student-facing view: questions and options only, never the right answers.</summary>
internal sealed class GetAssessmentHandler(ISmartLmsDbContext context) : IQueryHandler<GetAssessmentQuery, AssessmentDetail>
{
    public async ValueTask<ErrorOr<AssessmentDetail>> Handle(GetAssessmentQuery query, CancellationToken cancellationToken)
    {
        var detailSpec = new ActiveAssessmentByIdSpec(query.Id);
        var detail = await SpecificationEvaluator.Default
            .GetQuery(context.Assessments.AsNoTracking(), detailSpec)
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
