using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Content;

public class Course : Entity
{
    public Course(string name, int order, Guid subjectId, Guid? teacherInChargeId = null, string? image = null)
    {
        Name = name;
        Order = order;
        SubjectId = subjectId;
        TeacherInChargeId = teacherInChargeId;
        Image = image;
    }

    // ORM materialization.
    private Course()
    {
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    private int _order;

    public int Order
    {
        get => _order;
        private set => _order = new NonNegativeInteger(value).Value;
    }

    private string? _image;

    /// <summary>Storage path or URL of the course cover image (optional).</summary>
    public string? Image
    {
        get => _image;
        private set
        {
            if (value is not null)
            {
                _image = value.Length > 0 ? new Url(value).Value : null;
            }
            else
            {
                _image = null;
            }
        }
    }

    public Guid SubjectId { get; private set; }

    public Subject Subject { get; private set; } = null!;

    public Guid? TeacherInChargeId { get; private set; }

    public Teacher? TeacherInCharge { get; private set; }

    public ICollection<Class> Classes { get; private set; } = [];

    public ICollection<Assessment> AvailableAssessments { get; private set; } = [];

    public ICollection<ClassroomCourse> Classrooms { get; private set; } = [];

    public void Rename(string name) => Name = name;

    public void SetOrder(int order) => Order = new NonNegativeInteger(order).Value;

    public void ChangeImage(string? image) => Image = image;

    public void MoveToSubject(Guid subjectId) => SubjectId = subjectId;

    public void AssignTeacher(Guid? teacherId) => TeacherInChargeId = teacherId;
}
