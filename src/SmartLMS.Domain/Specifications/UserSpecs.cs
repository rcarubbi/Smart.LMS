using Ardalis.Specification;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Specifications;

/// <summary>Filters users by role (optional). Omission keeps the admin list complete (incl. inactive).</summary>
public sealed class UsersByRoleSpec : Specification<User>
{
    public UsersByRoleSpec(UserRole? role = null, bool? active = null)
    {
        if (active.HasValue)
        {
            Query.Where(u => u.Active == active.Value);
        }

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

/// <summary>User lookup by email for login (active check stays in handler to keep InvalidCredentials semantics).</summary>
public sealed class UserByEmailSpec : Specification<User>
{
    public UserByEmailSpec(string email)
    {
        Query.Where(u => u.Email == email);
    }
}

/// <summary>Active users only, optionally restricted to a single role.</summary>
public sealed class ActiveUsersByRoleSpec : Specification<User>
{
    public ActiveUsersByRoleSpec(UserRole? role = null)
        : this(role, true)
    {
    }

    private ActiveUsersByRoleSpec(UserRole? role, bool active)
        : base()
    {
        Query.Where(u => u.Active == active);

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
