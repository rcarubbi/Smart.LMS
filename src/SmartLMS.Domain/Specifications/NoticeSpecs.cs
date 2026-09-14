using Ardalis.Specification;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.Domain.Specifications;

/// <summary>My notices: active + (personal to user or broadcast), newest first.</summary>
public sealed class MyNoticesSpec : Specification<Notice>
{
    public MyNoticesSpec(Guid userId)
    {
        Query.Where(n => n.Active && (n.UserId == userId || n.UserId == null));
        Query.OrderByDescending(n => n.DateTime);
    }
}
