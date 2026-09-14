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
        classrooms.MapGet("/", async (bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListClassroomsQuery(active, page, pageSize), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        classrooms.MapPost("/", async (ClassroomInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateClassroomCommand(request), ct);
                return r.ToResult(id => Results.Created($"/api/classrooms/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapPut("/{id:guid}", async (Guid id, ClassroomInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new UpdateClassroomCommand(id, request), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteClassroomCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly);

        classrooms.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RestoreClassroomCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        classrooms.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new DeleteClassroomPermanentlyCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        classrooms.MapPut("/{id:guid}/courses", async (Guid id, EnrollCoursesInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new EnrollClassroomCoursesCommand(id, request), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(409);

        var delivery = app.MapGroup("/api/delivery-plans").WithTags("Delivery");
        delivery.MapGet("/", async (int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListDeliveryPlansQuery(page, pageSize), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200);

        delivery.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new GetDeliveryPlanQuery(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200)
            .Produces(404);

        delivery.MapPost("/", async (CreateDeliveryPlanInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new CreateDeliveryPlanCommand(request), ct);
                return r.ToResult(id => Results.Created($"/api/delivery-plans/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        // Manual trigger of the delivery job (also invoked hourly by the TickerQ worker).
        delivery.MapPost("/run", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new ExecuteDeliveriesCommand(), ct);
                return r.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200);

        var notices = app.MapGroup("/api/notices").WithTags("Notices");
        notices.MapGet("/", async (int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new ListMyNoticesQuery(page, pageSize), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        notices.MapPost("/", async (SendNoticeInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new SendNoticeCommand(request), ct);
                return r.ToResult(id => Results.Created($"/api/notices/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400);

        notices.MapPost("/{id:guid}/seen", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new MarkNoticeSeenCommand(id), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        var history = app.MapGroup("/api/me").WithTags("Me");
        history.MapGet("/progress", async (Guid? classId, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new MyProgressQuery(classId, page, pageSize), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        history.MapGet("/classrooms", async (int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Query(new MyClassroomQuery(page, pageSize), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        history.MapPost("/class-access", async (RecordClassAccessInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RecordClassAccessCommand(request), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        history.MapPost("/file-access", async (RecordFileAccessInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var r = await dispatcher.Send(new RecordFileAccessCommand(request), ct);
                return r.ToResult();
            })
            .RequireAuthorization();

        return app;
    }
}
