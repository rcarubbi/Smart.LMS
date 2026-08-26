using SmartLMS.DataAccess;

namespace SmartLMS.Worker;

/// <summary>All worker activity is attributed to the system (null user).</summary>
internal sealed class SystemCurrentUserProvider : ICurrentUserProvider
{
    public Guid? GetCurrentUserId() => null;
}
