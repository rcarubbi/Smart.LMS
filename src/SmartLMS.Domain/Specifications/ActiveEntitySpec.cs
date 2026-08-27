using Ardalis.Specification;
using SmartLMS.Domain.Entities;

namespace SmartLMS.Domain.Specifications;

/// <summary>Selects only entities that are still active (not soft-deleted).</summary>
public sealed class ActiveEntitySpec<T> : Specification<T>
    where T : Entity
{
    public ActiveEntitySpec() => Query.Where(e => e.Active);
}
