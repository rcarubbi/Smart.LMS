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

        group.MapPost("/login", async (LoginRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new LoginCommand(request.Email, request.Password), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .AllowAnonymous()
            .Produces(200)
            .Produces(401)
            .Produces(400);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RefreshTokenCommand(request.RefreshToken), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .AllowAnonymous()
            .Produces(200)
            .Produces(401);

        group.MapPost("/logout", async (LogoutRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new LogoutCommand(request.RefreshToken), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(204);

        group.MapPost("/change-password", async (ChangePasswordRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(204)
            .Produces(400)
            .Produces(401);

        return app;
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
