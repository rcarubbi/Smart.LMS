using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Student : User
{
    public Student(string name, string email)
        : base(name, email)
    {
    }

    // ORM materialization.
    private Student()
    {
    }

    public ICollection<DeliveryPlan> DeliveryPlans { get; private set; } = [];
}
