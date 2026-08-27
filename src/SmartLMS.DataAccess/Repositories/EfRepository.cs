using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

namespace SmartLMS.DataAccess.Repositories;

/// <summary>
/// Generic Ardalis.Specification repository backed by the EF Core DbContext.
/// Provides GetByIdAsync / ListAsync(spec) / CountAsync(spec) plus add/delete.
/// </summary>
public sealed class EfRepository<T>(SmartLmsDbContext dbContext) : RepositoryBase<T>(dbContext), IReadRepositoryBase<T>, IRepositoryBase<T>
    where T : class
{
}
