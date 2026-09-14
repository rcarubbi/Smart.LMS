using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.UnitTests.Domain;

public class DeliveryPlanTests
{
    private static readonly DateTime StartDate = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    private static DeliveryPlan CreatePlan(Classroom classroom)
    {
        var plan = new DeliveryPlan(classroom.Id, StartDate);
        plan.AttachClassroom(classroom);
        return plan;
    }

    [Test]
    public async Task GetSchedule_computes_cumulative_dates_across_ordered_courses_and_classes()
    {
        var classroom = new ClassroomBuilder()
            .WithCourse("Course A", order: 1,
                c => c.WithClass("A1", deliveryDays: 1, classOrder: 2)
                      .WithClass("A2", deliveryDays: 3, classOrder: 1))
            .WithCourse("Course B", order: 2,
                c => c.WithClass("B1", deliveryDays: 5, classOrder: 1))
            .Build();

        var plan = CreatePlan(classroom);

        var schedule = plan.GetSchedule();

        // Classes are ordered inside each course (A2 before A1); courses by Order.
        await Assert.That(schedule.Select(s => s.DeliveryDate.Date)).IsEquivalentTo(
        [
            StartDate.AddDays(3).Date, // A2
            StartDate.AddDays(4).Date, // A1 (cumulative)
            StartDate.AddDays(9).Date, // B1
        ]);
    }

    [Test]
    public async Task GetPendingClasses_returns_only_due_undelivered_classes()
    {
        var classroom = new ClassroomBuilder()
            .WithCourse("Course", order: 1,
                c => c.WithClass("C1", deliveryDays: 1)
                      .WithClass("C2", deliveryDays: 10))
            .Build();

        var plan = CreatePlan(classroom);
        var today = DateOnly.FromDateTime(StartDate.AddDays(5));

        var pending = plan.GetPendingClasses(today);

        var pendingItem = pending.Single();
        await Assert.That(pendingItem.ClassId).IsEqualTo(classroom.Courses.Single().Course.Classes.Single(c => c.Name == "C1").Id);
    }

    [Test]
    public async Task MarkDelivered_adds_entry_and_concludes_when_all_delivered()
    {
        var classroom = new ClassroomBuilder()
            .WithCourse("Course", order: 1,
                c => c.WithClass("C1", deliveryDays: 1)
                      .WithClass("C2", deliveryDays: 2))
            .Build();

        var plan = CreatePlan(classroom);
        var ids = classroom.Courses.Single().Course.Classes.OrderBy(c => c.Order).Select(c => c.Id).ToList();

        plan.MarkDelivered(ids[0]);
        await Assert.That(plan.Concluded).IsFalse();

        plan.MarkDelivered(ids[1]);
        await Assert.That(plan.Concluded).IsTrue();
        await Assert.That(plan.DeliveredClasses.Count).IsEqualTo(2);
    }

    [Test]
    public async Task MarkDelivered_is_idempotent_on_duplicate_class()
    {
        var classroom = new ClassroomBuilder()
            .WithCourse("Course", order: 1, c => c.WithClass("C1", deliveryDays: 1))
            .Build();

        var plan = CreatePlan(classroom);
        var classId = classroom.Courses.Single().Course.Classes.Single().Id;

        var first = plan.MarkDelivered(classId);
        var second = plan.MarkDelivered(classId);

        using (Assert.Multiple())
        {
            await Assert.That(second.ClassId).IsEqualTo(classId);
            await Assert.That(plan.DeliveredClasses.Count).IsEqualTo(1);
            await Assert.That(plan.Concluded).IsTrue();
        }

        _ = first;
    }

    [Test]
    public async Task GetPendingClasses_returns_empty_when_plan_is_concluded()
    {
        var classroom = new ClassroomBuilder()
            .WithCourse("Course", order: 1, c => c.WithClass("C1", deliveryDays: 1))
            .Build();

        var plan = CreatePlan(classroom);
        var classId = classroom.Courses.Single().Course.Classes.Single().Id;
        plan.MarkDelivered(classId);

        var pending = plan.GetPendingClasses(DateOnly.FromDateTime(StartDate.AddYears(1)));

        await Assert.That(pending).IsEmpty();
    }
}

/// <summary>Fluent builder for the Classroom -> Courses -> Classes graph.</summary>
internal sealed class ClassroomBuilder
{
    private readonly List<(string Name, int Order, List<(string Name, int DeliveryDays, int ClassOrder)> Classes)> _courses = [];

    public ClassroomBuilder WithCourse(string name, int order, Func<CourseBuilder, CourseBuilder> configure)
    {
        var builder = configure(new CourseBuilder());
        _courses.Add((name, order, builder.Classes));
        return this;
    }

    public Classroom Build()
    {
        var classroom = new Classroom("Classroom");
        var order = 0;
        var teacherId = Guid.NewGuid();

        foreach (var (courseName, courseOrder, classes) in _courses)
        {
            var course = new Course(courseName, courseOrder, Guid.NewGuid());

            foreach (var (className, deliveryDays, classOrder) in classes.OrderBy(c => c.ClassOrder))
            {
                course.Classes.Add(new Class(
                    className,
                    "https://example.com/lesson",
                    classOrder,
                    deliveryDays,
                    ContentType.YouTube,
                    course.Id,
                    teacherId));
            }

            var classroomCourse = new ClassroomCourse(classroom.Id, course.Id, order++);
            classroomCourse.AttachClassroom(classroom);
            classroomCourse.AttachCourse(course);
            classroom.Courses.Add(classroomCourse);
        }

        return classroom;
    }
}

internal sealed class CourseBuilder
{
    public readonly List<(string Name, int DeliveryDays, int ClassOrder)> Classes = [];

    public CourseBuilder WithClass(string name, int deliveryDays, int classOrder = 1)
    {
        Classes.Add((name, deliveryDays, classOrder));
        return this;
    }
}
