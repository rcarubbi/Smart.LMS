using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Content;

public class Class : Entity
{
    public Class(string name, string content, int order, int deliveryDays, ContentType contentType, Guid courseId, Guid teacherId)
    {
        Name = name;
        Content = content;
        Order = order;
        DeliveryDays = deliveryDays;
        ContentType = contentType;
        CourseId = courseId;
        TeacherId = teacherId;
    }

    // ORM materialization.
    private Class()
    {
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    private string _content = string.Empty;

    /// <summary>Video URL (Vimeo/YouTube) of the lesson.</summary>
    public string Content
    {
        get => _content;
        private set => _content = new Url(value).Value;
    }

    private int _order;

    public int Order
    {
        get => _order;
        private set => _order = new NonNegativeInteger(value).Value;
    }

    private int _deliveryDays;

    /// <summary>Days after the delivery plan start date when this class becomes available.</summary>
    public int DeliveryDays
    {
        get => _deliveryDays;
        private set => _deliveryDays = new NonNegativeInteger(value).Value;
    }

    public ContentType ContentType { get; private set; }

    public Guid CourseId { get; private set; }

    public Course Course { get; private set; } = null!;

    public Guid TeacherId { get; private set; }

    public Teacher Teacher { get; private set; } = null!;

    public ICollection<Comment> Comments { get; private set; } = [];

    public ICollection<ClassAccess> Accesses { get; private set; } = [];

    public ICollection<ClassDeliveryPlan> DeliveredPlans { get; private set; } = [];

    public ICollection<SupportFile> Files { get; private set; } = [];

    public void Rename(string name) => Name = name;

    public void SetOrder(int order) => Order = new NonNegativeInteger(order).Value;

    public void ChangeDeliveryDays(int days) => DeliveryDays = new NonNegativeInteger(days).Value;

    public void ChangeContent(string url) => Content = url;

    public void SetContentType(ContentType type) => ContentType = type;

    public void AssignTeacher(Guid teacherId) => TeacherId = teacherId;
}
