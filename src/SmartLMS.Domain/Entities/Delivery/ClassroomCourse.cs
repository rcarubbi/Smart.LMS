using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Delivery;

public class ClassroomCourse
{
    public ClassroomCourse(Guid classroomId, Guid courseId, int order)
    {
        ClassroomId = classroomId;
        CourseId = courseId;
        Order = order;
    }

    // ORM materialization.
    private ClassroomCourse()
    {
    }

    public Guid CourseId { get; private set; }

    private Course _course = null!;

    public Course Course
    {
        get => _course;
        private set => _course = value;
    }

    public Guid ClassroomId { get; private set; }

    private Classroom _classroom = null!;

    public Classroom Classroom
    {
        get => _classroom;
        private set => _classroom = value;
    }

    private int _order;

    /// <summary>Position of the course inside the classroom delivery sequence.</summary>
    public int Order
    {
        get => _order;
        private set => _order = new NonNegativeInteger(value).Value;
    }

    public void SetOrder(int order) => Order = order;

    public void AttachCourse(Course course)
    {
        Course = course;
        CourseId = course.Id;
    }

    public void AttachClassroom(Classroom classroom)
    {
        Classroom = classroom;
        ClassroomId = classroom.Id;
    }
}
