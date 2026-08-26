using Microsoft.EntityFrameworkCore;

namespace SmartLMS.DataAccess;

/// <summary>Resolves the user responsible for the current persistence operation.</summary>
public interface ICurrentUserProvider
{
    /// <summary>Id of the authenticated user, or null for system/background operations.</summary>
    Guid? GetCurrentUserId();
}
