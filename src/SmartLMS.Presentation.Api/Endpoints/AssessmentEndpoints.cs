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
                return r.ToResult();
            })
            .RequireAuthorization()
            .Produces(404);

        group.MapPost("/", async (CreateAssessmentInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateAssessmentCommand(request), ct);
                return r.ToResult(id => Results.Created($"/api/assessments/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAssessmentInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new UpdateAssessmentCommand(id, request), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapGet("/{id:guid}/edit", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new GetAssessmentForEditQuery(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200)
            .Produces(404);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteAssessmentCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RestoreAssessmentCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteAssessmentPermanentlyCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapPost("/{id:guid}/submit", async (Guid id, SubmitAssessmentInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new SubmitAssessmentCommand(id, request), ct);
                return r.ToResult();
            })
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        app.MapGet("/api/courses/{courseId:guid}/assessments",
                async (Guid courseId, bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListAssessmentsQuery(courseId, active, page, pageSize), ct);
                return r.ToResult();
            })
            .WithTags("Assessments")
            .RequireAuthorization();

        return app;
    }
}
