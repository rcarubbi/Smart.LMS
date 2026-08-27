using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Delivery;

public class Classroom : Entity
{
    public Classroom(string name) => Name = name;

    // ORM materialization.
    private Classroom()
    {
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    /// <summary>Ordered list of courses delivered to this classroom.</summary>
    public ICollection<ClassroomCourse> Courses { get; private set; } = [];

    public ICollection<DeliveryPlan> DeliveryPlans { get; private set; } = [];

    public void Rename(string name) => Name = name;
}
