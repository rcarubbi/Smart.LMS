using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Assessments;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class AssessmentEndpoints
{
    public static IEndpointRouteBuilder MapAssessmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assessments").WithTags("Assessments");

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new GetAssessmentQuery(id), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization()
            .Produces(404);

        group.MapPost("/", async (CreateRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateAssessmentCommand(
                    request.CourseId, request.MinimumScore, request.Questions), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Created($"/api/assessments/{r.Value}", new { id = r.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteAssessmentCommand(id), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly);

        group.MapPost("/{id:guid}/submit", async (Guid id, SubmitRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var submissions = request.Questions
                    .Select(q => new QuestionSubmission(q.QuestionId, q.OptionIds))
                    .ToList();

                var r = await dispatcher.Send(new SubmitAssessmentCommand(id, submissions), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        app.MapGet("/api/courses/{courseId:guid}/assessments",
                async (Guid courseId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListAssessmentsQuery(courseId), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .WithTags("Assessments")
            .RequireAuthorization();

        return app;
    }
}

public sealed record CreateRequest(Guid CourseId, int MinimumScore, IReadOnlyList<QuestionInput> Questions);

public sealed record SubmitRequest(IReadOnlyList<QuestionSubmissionBody> Questions);

public sealed record QuestionSubmissionBody(Guid QuestionId, IReadOnlyList<Guid> OptionIds);
