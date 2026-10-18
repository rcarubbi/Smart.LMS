using Ardalis.Specification;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.Domain.Specifications;

/// <summary>My notices: personal, global broadcasts, or broadcasts for enrolled delivery plans.</summary>
public sealed class MyNoticesSpec : Specification<Notice>
{
    public MyNoticesSpec(Guid userId)
    {
        Query.Where(n => n.Active && (n.UserId == userId ||
            (n.UserId == null && (n.DeliveryPlanId == null || n.DeliveryPlan!.Students.Any(s => s.Id == userId)))));
        Query.OrderByDescending(n => n.DateTime);
    }
}
