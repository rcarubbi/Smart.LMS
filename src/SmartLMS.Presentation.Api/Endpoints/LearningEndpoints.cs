using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Learning.Classrooms;
using SmartLMS.Application.Learning.Delivery;
using SmartLMS.Application.Learning.History;
using SmartLMS.Application.Learning.Notices;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class LearningEndpoints
{
    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder app)
    {
        var classrooms = app.MapGroup("/api/classrooms").WithTags("Classrooms");
        classrooms.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListClassroomsQuery(), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization();

        classrooms.MapPost("/", async (ClassroomRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateClassroomCommand(request.Name), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Created($"/api/classrooms/{r.Value}", new { id = r.Value });
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapPut("/{id:guid}", async (Guid id, ClassroomRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new UpdateClassroomCommand(id, request.Name), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteClassroomCommand(id), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapPut("/{id:guid}/courses", async (Guid id, EnrollCoursesRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new EnrollClassroomCoursesCommand(id, request.CourseIds), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(409);

        var delivery = app.MapGroup("/api/delivery-plans").WithTags("Delivery");
        delivery.MapPost("/", async (CreatePlanRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateDeliveryPlanCommand(request.ClassroomId, request.StartDate, request.StudentIds), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Created($"/api/delivery-plans/{r.Value}", new { id = r.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        // Manual trigger of the delivery job (also invoked hourly by the TickerQ worker).
        delivery.MapPost("/run", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new ExecuteDeliveriesCommand(), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200);

        var notices = app.MapGroup("/api/notices").WithTags("Notices");
        notices.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListMyNoticesQuery(), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization();

        notices.MapPost("/", async (SendNoticeRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new SendNoticeCommand(request.Text), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Created($"/api/notices/{r.Value}", new { id = r.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400);

        notices.MapPost("/{id:guid}/seen", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new MarkNoticeSeenCommand(id), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization();

        var history = app.MapGroup("/api/me").WithTags("Me");
        history.MapGet("/progress", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new MyProgressQuery(), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization();

        history.MapGet("/classrooms", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new MyClassroomQuery(), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.Ok(r.Value);
            })
            .RequireAuthorization();

        history.MapPost("/class-access", async (ClassAccessRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RecordClassAccessCommand(request.ClassId, request.Percentual, request.WatchedSeconds), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization();

        history.MapPost("/file-access", async (FileAccessRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RecordFileAccessCommand(request.FileId), ct);
                return r.IsError ? r.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization();

        return app;
    }
}

public sealed record ClassroomRequest(string Name);

public sealed record EnrollCoursesRequest(IReadOnlyList<Guid> CourseIds);

public sealed record CreatePlanRequest(Guid ClassroomId, DateTime StartDate, IReadOnlyList<Guid> StudentIds);

public sealed record SendNoticeRequest(string Text);

public sealed record ClassAccessRequest(Guid ClassId, int Percentual, decimal WatchedSeconds);

public sealed record FileAccessRequest(Guid FileId);
