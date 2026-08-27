using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Delivery;

/// <summary>
/// Scheduled release of a classroom's courses to its students.
/// Pure scheduling rules live here; orchestration (notices, e-mails, persistence)
/// belongs to the application layer.
/// </summary>
public class DeliveryPlan : Entity
{
    public DeliveryPlan(Guid classroomId, DateTime startDate)
    {
        ClassroomId = classroomId;
        StartDate = startDate;
    }

    // ORM materialization.
    private DeliveryPlan()
    {
    }

    private DateTime _startDate;

    public DateTime StartDate
    {
        get => _startDate;
        private set => _startDate = value;
    }

    private bool _concluded;

    public bool Concluded
    {
        get => _concluded;
        private set => _concluded = value;
    }

    public Guid ClassroomId { get; private set; }

    private Classroom _classroom = null!;

    public Classroom Classroom
    {
        get => _classroom;
        private set => _classroom = value;
    }

    public ICollection<Student> Students { get; private set; } = [];

    /// <summary>Classes already made available to students.</summary>
    public ICollection<ClassDeliveryPlan> DeliveredClasses { get; private set; } = [];

    public void AddStudent(Student student) => Students.Add(student);

    public void AttachClassroom(Classroom classroom)
    {
        Classroom = classroom;
        ClassroomId = classroom.Id;
    }

    /// <summary>
    /// Computes the full delivery schedule: courses in classroom order, classes in course order,
    /// each class due <c>DeliveryDays</c> after the previous one (cumulative), starting at <see cref="StartDate"/>.
    /// </summary>
    public IReadOnlyList<ScheduledClass> GetSchedule()
    {
        var schedule = new List<ScheduledClass>();
        var lastDate = StartDate;

        foreach (var classroomCourse in Classroom.Courses.OrderBy(c => c.Order))
        {
            foreach (var klass in classroomCourse.Course.Classes.OrderBy(c => c.Order))
            {
                var deliveryDate = lastDate.AddDays(klass.DeliveryDays);
                schedule.Add(new ScheduledClass(klass.Id, deliveryDate));
                lastDate = deliveryDate;
            }
        }

        return schedule;
    }

    /// <summary>Schedule entries due on or before <paramref name="today"/> that have not been delivered yet.</summary>
    public IReadOnlyList<ScheduledClass> GetPendingClasses(DateOnly today)
    {
        var deliveredIds = DeliveredClasses.Select(d => d.ClassId).ToHashSet();
        var pending = GetSchedule()
            .Where(s => DateOnly.FromDateTime(s.DeliveryDate) <= today && !deliveredIds.Contains(s.ClassId))
            .ToList();

        if (!Concluded)
        {
            return pending;
        }

        return [];
    }

    /// <summary>Marks a class as delivered. Flags the plan as concluded when every scheduled class is delivered.</summary>
    public ClassDeliveryPlan MarkDelivered(Guid classId)
    {
        if (DeliveredClasses.Any(d => d.ClassId == classId))
        {
            throw new InvalidOperationException($"Class '{classId}' is already delivered by this plan.");
        }

        var entry = new ClassDeliveryPlan(classId, DateTime.UtcNow, this);

        DeliveredClasses.Add(entry);

        var totalScheduled = GetSchedule().Count;
        Concluded = DeliveredClasses.Count >= totalScheduled;

        return entry;
    }
}

public readonly record struct ScheduledClass(Guid ClassId, DateTime DeliveryDate);
