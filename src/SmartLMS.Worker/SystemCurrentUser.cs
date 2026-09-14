using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Worker;

/// <summary>All worker activity is attributed to the system (null user, no role).</summary>
internal sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;

    public UserRole? Role => null;
}
