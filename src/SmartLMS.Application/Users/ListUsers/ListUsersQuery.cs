using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Users.ListUsers;

public sealed record UserListItem(Guid Id, string Name, string Email, string Role, bool Active);

public sealed record ListUsersQuery(UserRole? Role) : IQuery<IReadOnlyList<UserListItem>>;

internal sealed class ListUsersHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListUsersQuery, IReadOnlyList<UserListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<UserListItem>>> Handle(
        ListUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = context.Users.AsNoTracking();

        if (query.Role is { } role)
        {
            users = role switch
            {
                UserRole.Admin => users.Where(u => u is Admin),
                UserRole.Teacher => users.Where(u => u is Teacher),
                UserRole.Student => users.Where(u => u is Student),
                _ => users,
            };
        }

        var items = await users
            .OrderBy(u => u.Name)
            .Select(u => new UserListItem(
                u.Id,
                u.Name,
                u.Email,
                u is Admin ? nameof(UserRole.Admin) : u is Teacher ? nameof(UserRole.Teacher) : nameof(UserRole.Student),
                u.Active))
            .ToListAsync(cancellationToken);

        return items;
    }
}
