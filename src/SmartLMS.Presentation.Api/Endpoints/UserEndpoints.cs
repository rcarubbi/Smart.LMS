using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.Application.Users.DeleteUser;
using SmartLMS.Application.Users.DeleteUserPermanently;
using SmartLMS.Application.Users.ListUsers;
using SmartLMS.Application.Users.RestoreUser;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (UserRole? role, bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListUsersQuery(role, active, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(200);

        group.MapPost("/", async (CreateUserInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateUserCommand(request), ct);

                return result.ToResult(id => Results.Created($"/api/users/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(403);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteUserCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RestoreUserCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteUserPermanentlyCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        return app;
    }
}
