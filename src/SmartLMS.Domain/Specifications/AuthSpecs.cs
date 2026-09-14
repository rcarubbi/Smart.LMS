using Ardalis.Specification;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Specifications;

/// <summary>Refresh token lookup by hash with user for rotation (tracking: revoke + save).</summary>
public sealed class RefreshTokenByHashWithUserSpec : Specification<RefreshToken>
{
    public RefreshTokenByHashWithUserSpec(string tokenHash)
    {
        Query.Where(t => t.TokenHash == tokenHash);
        Query.Include(t => t.User);
    }
}

/// <summary>Active (non-revoked) refresh token by hash for idempotent logout.</summary>
public sealed class ActiveRefreshTokenByHashSpec : Specification<RefreshToken>
{
    public ActiveRefreshTokenByHashSpec(string tokenHash)
    {
        Query.Where(t => t.TokenHash == tokenHash && t.RevokedAtUtc == null);
    }
}
