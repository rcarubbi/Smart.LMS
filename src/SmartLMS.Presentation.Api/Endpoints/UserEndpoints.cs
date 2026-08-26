using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/", async (CreateUserRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateUserCommand(request.Name, request.Email, request.Password, request.Role),
                    ct);

                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                    : result.Error.ToProblem();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(403);

        return app;
    }
}

public sealed record CreateUserRequest(string Name, string Email, string Password, UserRole Role);
