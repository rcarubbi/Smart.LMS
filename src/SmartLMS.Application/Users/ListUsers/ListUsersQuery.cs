using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Users.ListUsers;

public sealed record UserListItem(Guid Id, string Name, string Email, string Role, bool Active);

public sealed record ListUsersQuery(
    UserRole? Role,
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<UserListItem>>;

internal sealed class ListUsersHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListUsersQuery, Paged<UserListItem>>
{
    public async ValueTask<ErrorOr<Paged<UserListItem>>> Handle(
        ListUsersQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new UsersByRoleSpec(query.Role, query.Active);
        var users = SpecificationEvaluator.Default.GetQuery(context.Users.AsNoTracking(), spec);

        var items = await users
            .Select(u => new UserListItem(
                u.Id,
                u.Name,
                u.Email,
                u is Admin ? nameof(UserRole.Admin) : u is Teacher ? nameof(UserRole.Teacher) : nameof(UserRole.Student),
                u.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
