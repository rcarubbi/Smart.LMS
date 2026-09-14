using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth.ChangePassword;
using SmartLMS.Application.Auth.Login;
using SmartLMS.Application.Auth.Logout;
using SmartLMS.Application.Auth.Refresh;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new LoginCommand(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .Produces(200)
            .Produces(401)
            .Produces(400);

        group.MapPost("/refresh", async (RefreshTokenInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RefreshTokenCommand(request), ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .Produces(200)
            .Produces(401);

        group.MapPost("/logout", async (LogoutInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new LogoutCommand(request), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(204);

        group.MapPost("/change-password", async (ChangePasswordInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new ChangePasswordCommand(request), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(204)
            .Produces(400)
            .Produces(401);

        return app;
    }
}
