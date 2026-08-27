using Ardalis.Specification;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Specifications;

/// <summary>Filters users by role (optional). Omission keeps the admin list complete (incl. inactive).</summary>
public sealed class UsersByRoleSpec : Specification<User>
{
    public UsersByRoleSpec(UserRole? role = null)
    {
        if (role is { } r)
        {
            Query.Where(u => r == UserRole.Admin ? u is Admin
                : r == UserRole.Teacher ? u is Teacher
                : r == UserRole.Student ? u is Student
                : true);
        }

        Query.OrderBy(u => u.Name);
    }
}

/// <summary>Active users only, optionally restricted to a single role unless <paramref name="adminIncludesInactive"/>.</summary>
public sealed class ActiveUsersByRoleSpec : Specification<User>
{
    public ActiveUsersByRoleSpec(UserRole? role = null)
    {
        Query.Where(u => u.Active);

        if (role is { } r)
        {
            Query.Where(u => r == UserRole.Admin ? u is Admin
                : r == UserRole.Teacher ? u is Teacher
                : r == UserRole.Student ? u is Student
                : true);
        }

        Query.OrderBy(u => u.Name);
    }
}
