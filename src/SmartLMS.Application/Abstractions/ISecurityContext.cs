using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Abstractions;

/// <summary>Port for issuing access/refresh tokens (implemented with JWT + random secrets).</summary>
public interface ITokenService
{
    AccessTokenInfo CreateAccessToken(Guid userId, string email, UserRole role);

    RefreshTokenMaterial CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}

public sealed record AccessTokenInfo(string Token, DateTime ExpiresAtUtc);

public sealed record RefreshTokenMaterial(string Token, DateTime ExpiresAtUtc);

/// <summary>Ambient user context resolved from the HTTP request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    UserRole? Role { get; }
}
