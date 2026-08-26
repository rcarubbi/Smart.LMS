using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Auth.Models;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Auth;

/// <summary>Issues an access + refresh token pair for a user, persisting the hashed refresh token.</summary>
internal sealed class AuthTokenIssuer(
    ISmartLmsDbContext context,
    ITokenService tokenService,
    TimeProvider timeProvider)
{
    public async ValueTask<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var role = ResolveRole(user);

        var access = tokenService.CreateAccessToken(user.Id, user.Email, role);
        var refreshMaterial = tokenService.CreateRefreshToken();

        context.RefreshTokens.Add(new RefreshToken(
            user.Id,
            tokenService.HashRefreshToken(refreshMaterial.Token),
            refreshMaterial.ExpiresAtUtc,
            now));

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            access.Token,
            access.ExpiresAtUtc,
            refreshMaterial.Token,
            refreshMaterial.ExpiresAtUtc);
    }

    public static UserRole ResolveRole(User user) => user switch
    {
        Admin => UserRole.Admin,
        Teacher => UserRole.Teacher,
        Student => UserRole.Student,
        _ => throw new InvalidOperationException($"Unknown user type '{user.GetType().Name}'."),
    };
}
