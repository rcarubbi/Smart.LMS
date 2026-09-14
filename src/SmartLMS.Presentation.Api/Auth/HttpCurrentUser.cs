using System.Security.Claims;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Presentation.Api.Auth;

/// <summary>Resolves the authenticated user from JWT claims (inbound claim mapping disabled).</summary>
internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private const string UserIdClaim = "sub";
    private const string RoleClaim = "role";

    public Guid? UserId =>
        Guid.TryParse(accessor.HttpContext?.User?.FindFirstValue(UserIdClaim), out var id) ? id : null;

    public UserRole? Role =>
        Enum.TryParse<UserRole>(accessor.HttpContext?.User?.FindFirstValue(RoleClaim), out var role) ? role : null;
}
