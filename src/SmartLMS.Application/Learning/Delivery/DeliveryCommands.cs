using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.Application.Learning.Delivery;

public sealed record CreateDeliveryPlanCommand(
    Guid ClassroomId,
    DateTime StartDate,
    IReadOnlyList<Guid> StudentIds) : ICommand<Guid>;

internal sealed class CreateDeliveryPlanValidator : AbstractValidator<CreateDeliveryPlanCommand>
{
    public CreateDeliveryPlanValidator()
    {
        RuleFor(c => c.ClassroomId).NotEmpty();
        RuleFor(c => c.StartDate).NotEmpty();
    }
}

internal sealed class CreateDeliveryPlanHandler(ISmartLmsDbContext context)
    : ICommandHandler<CreateDeliveryPlanCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateDeliveryPlanCommand command, CancellationToken cancellationToken)
    {
        var classroomExists = await context.Classrooms.AnyAsync(c => c.Id == command.ClassroomId, cancellationToken);
        if (!classroomExists)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var students = await context.Users
            .Where(u => command.StudentIds.Contains(u.Id) && u is Domain.Entities.UserAccess.Student)
            .OfType<Domain.Entities.UserAccess.Student>()
            .ToListAsync(cancellationToken);

        if (students.Count != command.StudentIds.Distinct().Count())
        {
            return Error.Conflict(
                "Delivery.UnknownStudents",
                "One or more informed ids are not registered students.");
        }

        var plan = new DeliveryPlan(command.ClassroomId, command.StartDate);

        foreach (var student in students)
        {
            plan.AddStudent(student);
        }

        context.DeliveryPlans.Add(plan);
        await context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}

public sealed record DeliveredClassInfo(Guid ClassId, string ClassName, string CourseName, int StudentCount);

/// <summary>
/// Executes every due delivery across all open plans (legacy ContentDeliveryJob logic).
/// Creates a broadcast notice per delivered class and notifies enrolled students by e-mail.
/// Returns the number of classes delivered.
/// </summary>
public sealed record ExecuteDeliveriesCommand : ICommand<IReadOnlyList<DeliveredClassInfo>>;

internal sealed class ExecuteDeliveriesHandler(
    ISmartLmsDbContext context,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<ExecuteDeliveriesHandler> logger) : ICommandHandler<ExecuteDeliveriesCommand, IReadOnlyList<DeliveredClassInfo>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<DeliveredClassInfo>>> Handle(
        ExecuteDeliveriesCommand command,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var plans = await context.DeliveryPlans
            .Include(p => p.Classroom)
            .Include(p => p.Classroom.Courses)
                .ThenInclude(cc => cc.Course)
                    .ThenInclude(c => c.Classes)
            .Include(p => p.Classroom.Courses)
                .ThenInclude(cc => cc.Course)
                    .ThenInclude(c => c.Subject)
            .Include(p => p.Students)
            .Where(p => !p.Concluded)
            .ToListAsync(cancellationToken);

        List<DeliveredClassInfo> delivered = [];

        foreach (var plan in plans)
        {
            foreach (var pending in plan.GetPendingClasses(today))
            {
                var klass = plan.Classroom.Courses
                    .SelectMany(cc => cc.Course.Classes)
                    .Single(c => c.Id == pending.ClassId);
                var course = klass.Course;

                plan.MarkDelivered(klass.Id);

                context.Notices.Add(new Notice(
                    $"<a href='/class/{klass.Id}'>{klass.Name}</a> is now available! <br />" +
                    $"<a href='/course/{course.Id}'>{course.Name}</a>",
                    timeProvider.GetUtcNow().UtcDateTime,
                    plan.Id));

                foreach (var student in plan.Students.Where(s => s.Active))
                {
                    try
                    {
                        await emailSender.SendAsync(
                            student.Email,
                            $"New class available: {klass.Name}",
                            $"Hello {student.Name},<br/>The class <b>{klass.Name}</b> of course <b>{course.Name}</b> ({course.Subject.Name}) has just been released in classroom {plan.Classroom.Name}.",
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // E-mail failure must never block content delivery.
                        logger.LogWarning(ex, "Failed to notify {Email} about class {ClassId}", student.Email, klass.Id);
                    }
                }

                delivered.Add(new DeliveredClassInfo(klass.Id, klass.Name, course.Name, plan.Students.Count));
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return delivered;
    }
}
