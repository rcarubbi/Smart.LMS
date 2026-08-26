using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Content;

public class Course : Entity
{
    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Storage path or URL of the course cover image.</summary>
    public string? Image { get; set; }

    public Guid SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    public Guid? TeacherInChargeId { get; set; }

    public Teacher? TeacherInCharge { get; set; }

    public ICollection<Class> Classes { get; set; } = [];

    public ICollection<SupportFile> Files { get; set; } = [];

    public ICollection<Assessment> AvailableAssessments { get; set; } = [];

    public ICollection<ClassroomCourse> Classrooms { get; set; } = [];
}
