using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Content;

public class Class : Entity
{
    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>Days after the delivery plan start date when this class becomes available.</summary>
    public int DeliveryDays { get; set; }

    public ContentType ContentType { get; set; }

    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public Guid TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = [];

    public ICollection<ClassAccess> Accesses { get; set; } = [];

    public ICollection<ClassDeliveryPlan> DeliveredPlans { get; set; } = [];

    public ICollection<SupportFile> Files { get; set; } = [];
}
