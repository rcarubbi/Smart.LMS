namespace SmartLMS.Domain.Entities.Delivery;

public class Classroom : Entity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Ordered list of courses delivered to this classroom.</summary>
    public ICollection<ClassroomCourse> Courses { get; set; } = [];

    public ICollection<DeliveryPlan> DeliveryPlans { get; set; } = [];
}
