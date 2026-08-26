using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Student : User
{
    public ICollection<DeliveryPlan> DeliveryPlans { get; set; } = [];
}
