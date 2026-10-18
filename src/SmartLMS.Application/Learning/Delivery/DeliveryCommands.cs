using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Events.Delivery;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Learning.Delivery;

public sealed record CreateDeliveryPlanInput(
    Guid ClassroomId,
    DateTime StartDate,
    IReadOnlyList<Guid> StudentIds);

public sealed record CreateDeliveryPlanCommand(CreateDeliveryPlanInput Input) : ICommand<Guid>;

internal sealed class CreateDeliveryPlanValidator : AbstractValidator<CreateDeliveryPlanCommand>
{
    public CreateDeliveryPlanValidator()
    {
        RuleFor(c => c.Input.ClassroomId).NotEmpty();
        RuleFor(c => c.Input.StartDate).NotEmpty();
    }
}

internal sealed class CreateDeliveryPlanHandler(ISmartLmsDbContext context)
    : ICommandHandler<CreateDeliveryPlanCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateDeliveryPlanCommand command, CancellationToken cancellationToken)
    {
        var classroomExists = await context.Classrooms.AnyAsync(c => c.Id == command.Input.ClassroomId, cancellationToken);
        if (!classroomExists)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var students = await context.Users
            .Where(u => command.Input.StudentIds.Contains(u.Id) && u is Domain.Entities.UserAccess.Student)
            .OfType<Domain.Entities.UserAccess.Student>()
            .ToListAsync(cancellationToken);

        if (students.Count != command.Input.StudentIds.Distinct().Count())
        {
            return Error.Conflict(
                "Delivery.UnknownStudents",
                "One or more informed ids are not registered students.");
        }

        var plan = new DeliveryPlan(command.Input.ClassroomId, command.Input.StartDate);

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

public sealed record DeliveryPlanListItem(
    Guid Id,
    Guid ClassroomId,
    string ClassroomName,
    string StartDate,
    int StudentCount,
    bool Concluded,
    int DeliveredClassCount);

public sealed record ListDeliveryPlansQuery(int? Page = null, int? PageSize = null)
    : IQuery<Paged<DeliveryPlanListItem>>;

internal sealed class ListDeliveryPlansHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListDeliveryPlansQuery, Paged<DeliveryPlanListItem>>
{
    public async ValueTask<ErrorOr<Paged<DeliveryPlanListItem>>> Handle(
        ListDeliveryPlansQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new DeliveryPlansListSpec();
        var filtered = SpecificationEvaluator.Default.GetQuery(context.DeliveryPlans.AsNoTracking(), spec);

        var items = await filtered
            .Select(p => new DeliveryPlanListItem(
                p.Id,
                p.ClassroomId,
                p.Classroom.Name,
                p.StartDate.ToString("dd/MM/yyyy"),
                p.Students.Count,
                p.Concluded,
                p.DeliveredClasses.Count))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}

public sealed record ScheduleItem(
    Guid ClassId,
    string CourseName,
    int CourseOrder,
    string ClassName,
    int ClassOrder,
    int DeliveryDays,
    string DeliveryDate,
    bool Delivered);

public sealed record DeliveryPlanDetail(
    Guid Id,
    Guid ClassroomId,
    string ClassroomName,
    string StartDate,
    int StudentCount,
    bool Concluded,
    IReadOnlyList<ScheduleItem> Classes,
    IReadOnlyList<DeliveryPlanStudentSummary> Students);

public sealed record DeliveryPlanStudentSummary(Guid Id, string Name, string Email, bool Active);

public sealed record GetDeliveryPlanQuery(Guid Id) : IQuery<DeliveryPlanDetail>;

public sealed record UpdateDeliveryPlanStudentsInput(IReadOnlyList<Guid> StudentIds);
public sealed record UpdateDeliveryPlanStudentsCommand(Guid PlanId, UpdateDeliveryPlanStudentsInput Input) : ICommand;

internal sealed class UpdateDeliveryPlanStudentsValidator : AbstractValidator<UpdateDeliveryPlanStudentsCommand>
{
    public UpdateDeliveryPlanStudentsValidator() => RuleFor(c => c.PlanId).NotEmpty();
}

internal sealed class UpdateDeliveryPlanStudentsHandler(ISmartLmsDbContext context)
    : ICommandHandler<UpdateDeliveryPlanStudentsCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateDeliveryPlanStudentsCommand command, CancellationToken cancellationToken)
    {
        var plan = await context.DeliveryPlans.Include(p => p.Students)
            .SingleOrDefaultAsync(p => p.Id == command.PlanId, cancellationToken);
        if (plan is null)
        {
            return Error.NotFound("DeliveryPlans.NotFound", "Plano de entrega não encontrado.");
        }

        var ids = command.Input.StudentIds.Distinct().ToHashSet();
        var existingIds = plan.Students.Select(s => s.Id).ToHashSet();
        var students = await context.Students
            .Where(s => ids.Contains(s.Id) && (s.Active || existingIds.Contains(s.Id)))
            .ToListAsync(cancellationToken);
        if (students.Count != ids.Count)
        {
            return Error.Validation("DeliveryPlans.InvalidStudents", "Selecione alunos ativos ou mantenha as inscrições inativas já existentes.");
        }

        foreach (var student in plan.Students.Where(s => !ids.Contains(s.Id)).ToList())
        {
            plan.Students.Remove(student);
        }
        foreach (var student in students.Where(s => plan.Students.All(current => current.Id != s.Id)))
        {
            plan.AddStudent(student);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}

internal sealed class GetDeliveryPlanHandler(ISmartLmsDbContext context)
    : IQueryHandler<GetDeliveryPlanQuery, DeliveryPlanDetail>
{
    public async ValueTask<ErrorOr<DeliveryPlanDetail>> Handle(
        GetDeliveryPlanQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new DeliveryPlanByIdWithGraphSpec(query.Id);
        var plan = await SpecificationEvaluator.Default
            .GetQuery(context.DeliveryPlans.AsNoTracking(), spec)
            .SingleOrDefaultAsync(cancellationToken);

        if (plan is null)
        {
            return Error.NotFound("DeliveryPlans.NotFound", "Plano de entrega não encontrado.");
        }

        var deliveredIds = plan.DeliveredClasses.Select(d => d.ClassId).ToHashSet();

        var classLookup = plan.Classroom.Courses
            .OrderBy(cc => cc.Order)
            .Select((cc, index) => (CourseOrder: index + 1, cc))
            .SelectMany(x => x.cc.Course.Classes
                .OrderBy(c => c.Order)
                .Select(k => (x.CourseOrder, Class: k)))
            .ToDictionary(x => x.Class.Id, x => x);

        var schedule = plan.GetSchedule()
            .Select(s =>
            {
                var (courseOrder, klass) = classLookup[s.ClassId];
                return new ScheduleItem(
                    klass.Id,
                    klass.Course.Name,
                    courseOrder,
                    klass.Name,
                    klass.Order,
                    klass.DeliveryDays,
                    s.DeliveryDate.ToString("dd/MM/yyyy"),
                    deliveredIds.Contains(klass.Id));
            })
            .ToList();

        return new DeliveryPlanDetail(
            plan.Id,
            plan.ClassroomId,
            plan.Classroom.Name,
            plan.StartDate.ToString("dd/MM/yyyy"),
            plan.Students.Count,
            plan.Concluded,
            schedule,
            plan.Students.OrderBy(s => s.Name)
                .Select(s => new DeliveryPlanStudentSummary(s.Id, s.Name, s.Email, s.Active))
                .ToList());
    }
}

/// <summary>
/// Executes every due delivery across all open plans (legacy ContentDeliveryJob logic).
/// Creates a broadcast notice per delivered class and notifies enrolled students by e-mail.
/// Returns the number of classes delivered.
/// </summary>
/// <summary>Owns its transaction: commits per plan instead of one giant run-wide transaction.</summary>
public sealed record ExecuteDeliveriesCommand : ICommand<IReadOnlyList<DeliveredClassInfo>>, IOwnsTransaction;

internal sealed class ExecuteDeliveriesHandler(
    ISmartLmsDbContext context,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ExecuteDeliveriesHandler> logger) : ICommandHandler<ExecuteDeliveriesCommand, IReadOnlyList<DeliveredClassInfo>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<DeliveredClassInfo>>> Handle(
        ExecuteDeliveriesCommand command,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        // Tracking query on purpose: MarkDelivered + Notices.Add + outbox
        // persist per plan below.
        var spec = new DeliveryPlansWithGraphSpec();
        var plans = await SpecificationEvaluator.Default
            .GetQuery(context.DeliveryPlans, spec)
            .ToListAsync(cancellationToken);

        List<DeliveredClassInfo> delivered = [];

        foreach (var plan in plans)
        {
            var pending = plan.GetPendingClasses(today);
            if (pending.Count == 0)
            {
                continue;
            }

            // One transaction per plan: delivery + notice + outbox commit atomically,
            // locks release early, and a failed plan doesn't roll back the run's progress.
            // Nested sends reuse the ambient transaction instead.
            var ownsTransaction = !unitOfWork.HasActiveTransaction;
            if (ownsTransaction)
            {
                await unitOfWork.BeginTransactionAsync(cancellationToken);
            }

            try
            {
                foreach (var item in pending)
                {
                    var klass = plan.Classroom.Courses
                        .SelectMany(cc => cc.Course.Classes)
                        .Single(c => c.Id == item.ClassId);
                    var course = klass.Course;

                    plan.MarkDelivered(klass.Id);

                    context.Notices.Add(new Notice(
                        $"{klass.Name} is now available! {course.Name}",
                        timeProvider.GetUtcNow().UtcDateTime,
                        plan.Id));

                    plan.RaiseDomainEvent(new ClassDeliveredDomainEvent(
                        plan.Id,
                        klass.Id,
                        klass.Name,
                        course.Id,
                        course.Name,
                        course.Subject.Name,
                        plan.Classroom.Name,
                        plan.Students
                            .Where(s => s.Active)
                            .Select(s => new ClassDeliveredRecipient(s.Id, s.Name, s.Email))
                            .ToList(),
                        timeProvider.GetUtcNow().UtcDateTime));

                    delivered.Add(new DeliveredClassInfo(klass.Id, klass.Name, course.Name, plan.Students.Count));
                }

                await context.SaveChangesAsync(cancellationToken);

                if (ownsTransaction)
                {
                    await unitOfWork.CommitAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                if (ownsTransaction)
                {
                    await unitOfWork.RollbackAsync(CancellationToken.None);
                }

                // At-least-progress: committed plans stay delivered; this one retries next run.
                delivered.RemoveAll(d => pending.Any(p => p.ClassId == d.ClassId));
                logger.LogWarning(ex, "Delivery failed for plan {PlanId}; will retry on the next run", plan.Id);
            }
        }

        return delivered;
    }
}
