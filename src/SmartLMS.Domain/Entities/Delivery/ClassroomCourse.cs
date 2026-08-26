using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Entities.Delivery;

public class ClassroomCourse
{
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public Guid ClassroomId { get; set; }

    public Classroom Classroom { get; set; } = null!;

    /// <summary>Position of the course inside the classroom delivery sequence.</summary>
    public int Order { get; set; }
}
