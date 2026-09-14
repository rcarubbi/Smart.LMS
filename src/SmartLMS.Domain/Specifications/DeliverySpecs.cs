using Ardalis.Specification;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.Domain.Specifications;

/// <summary>
/// All delivery plans with the graph needed by <c>ExecuteDeliveries</c>:
/// classroom + courses + classes + subject + delivered + students.
/// Centralizes the 5x Include chain so handlers don't repeat it.
/// </summary>
public sealed class DeliveryPlansWithGraphSpec : Specification<DeliveryPlan>
{
    public DeliveryPlansWithGraphSpec()
    {
        Query.Include(p => p.Classroom);
        Query.Include(p => p.DeliveredClasses);
        Query.Include(p => p.Students);
        Query.Include("Classroom.Courses.Course.Classes");
        Query.Include("Classroom.Courses.Course.Subject");
    }
}

/// <summary>Delivery plans ordered for admin grids.</summary>
public sealed class DeliveryPlansListSpec : Specification<DeliveryPlan>
{
    public DeliveryPlansListSpec()
    {
        Query.OrderByDescending(p => p.StartDate);
    }
}

/// <summary>Single delivery plan with detail graph (classroom/courses/students/delivered).</summary>
public sealed class DeliveryPlanByIdWithGraphSpec : Specification<DeliveryPlan>
{
    public DeliveryPlanByIdWithGraphSpec(Guid id)
    {
        Query.Where(p => p.Id == id);
        Query.Include(p => p.Classroom);
        Query.Include(p => p.DeliveredClasses);
        Query.Include(p => p.Students);
        Query.Include("Classroom.Courses.Course.Classes");
        Query.Include("Classroom.Courses.Course.Subject");
    }
}

/// <summary>Classrooms list with optional active filter, ordered for admin grids.</summary>
public sealed class ClassroomsListSpec : Specification<Classroom>
{
    public ClassroomsListSpec(bool? active = null)
    {
        if (active.HasValue)
        {
            Query.Where(c => c.Active == active.Value);
        }

        Query.OrderBy(c => c.Name);
    }
}

/// <summary>Single classroom with ordered courses for enrollment.</summary>
public sealed class ClassroomWithCoursesSpec : Specification<Classroom>
{
    public ClassroomWithCoursesSpec(Guid id)
    {
        Query.Where(c => c.Id == id);
        Query.Include(c => c.Courses);
    }
}
