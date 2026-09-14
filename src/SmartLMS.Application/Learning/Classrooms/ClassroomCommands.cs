using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Specifications;
using ErrorOr;

namespace SmartLMS.Application.Learning.Classrooms;

public sealed record ClassroomInput(string Name);

public sealed record CreateClassroomCommand(ClassroomInput Input) : ICommand<Guid>;

internal sealed class CreateClassroomValidator : AbstractValidator<CreateClassroomCommand>
{
    public CreateClassroomValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<CreateClassroomCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.Classrooms.AnyAsync(c => c.Name == command.Input.Name, cancellationToken);
        if (duplicated)
        {
            return Error.Conflict("Classrooms.NameAlreadyExists", $"A classroom named '{command.Input.Name}' already exists.");
        }

        var classroom = new Domain.Entities.Delivery.Classroom(command.Input.Name);
        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return classroom.Id;
    }
}

public sealed record UpdateClassroomCommand(Guid Id, ClassroomInput Input) : ICommand;

internal sealed class UpdateClassroomValidator : AbstractValidator<UpdateClassroomCommand>
{
    public UpdateClassroomValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class UpdateClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateClassroomCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        classroom.Rename(command.Input.Name);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteClassroomCommand(Guid Id) : ICommand;

internal sealed class DeleteClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteClassroomCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var planIds = await context.DeliveryPlans
            .Where(p => p.ClassroomId == command.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (planIds.Count > 0)
        {
            var notices = await context.Notices
                .Where(n => n.DeliveryPlanId != null && planIds.Contains(n.DeliveryPlanId.Value))
                .ToListAsync(cancellationToken);
            foreach (var notice in notices)
            {
                notice.DetachDeliveryPlan();
            }

            var plans = await context.DeliveryPlans
                .Where(p => planIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            context.DeliveryPlans.RemoveRange(plans);
        }

        classroom.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreClassroomCommand(Guid Id) : ICommand;

internal sealed class RestoreClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreClassroomCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        classroom.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteClassroomPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteClassroomPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteClassroomPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(
        DeleteClassroomPermanentlyCommand command,
        CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var planIds = await context.DeliveryPlans
            .Where(p => p.ClassroomId == command.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (planIds.Count > 0)
        {
            var notices = await context.Notices
                .Where(n => n.DeliveryPlanId != null && planIds.Contains(n.DeliveryPlanId.Value))
                .ToListAsync(cancellationToken);
            foreach (var notice in notices)
            {
                notice.DetachDeliveryPlan();
            }

            var plans = await context.DeliveryPlans
                .Where(p => planIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
            context.DeliveryPlans.RemoveRange(plans);
        }

        context.Classrooms.Remove(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record ClassroomListItem(
    Guid Id,
    string Name,
    int CourseCount,
    int PlanCount,
    bool Active,
    IReadOnlyList<Guid> CourseIds);

public sealed record ListClassroomsQuery(
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<ClassroomListItem>>;

internal sealed class ListClassroomsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListClassroomsQuery, Paged<ClassroomListItem>>
{
    public async ValueTask<ErrorOr<Paged<ClassroomListItem>>> Handle(
        ListClassroomsQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new ClassroomsListSpec(query.Active);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.Classrooms.AsNoTracking(), spec);

        var items = await filtered
            .Select(c => new ClassroomListItem(
                c.Id,
                c.Name,
                c.Courses.Count,
                c.DeliveryPlans.Count,
                c.Active,
                c.Courses.OrderBy(cc => cc.Order).Select(cc => cc.CourseId).ToList()))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}

/// <summary>Replaces the ordered course list of a classroom.</summary>
public sealed record EnrollCoursesInput(IReadOnlyList<Guid> CourseIds);

public sealed record EnrollClassroomCoursesCommand(Guid ClassroomId, EnrollCoursesInput Input) : ICommand;

internal sealed class EnrollClassroomCoursesHandler(ISmartLmsDbContext context)
    : ICommandHandler<EnrollClassroomCoursesCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(EnrollClassroomCoursesCommand command, CancellationToken cancellationToken)
    {
        var spec = new ClassroomWithCoursesSpec(command.ClassroomId);
        var classroom = await SpecificationEvaluator.Default
            .GetQuery(context.Classrooms, spec)
            .SingleOrDefaultAsync(cancellationToken);

        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var requested = command.Input.CourseIds.Distinct().ToList();
        var existing = await context.Courses
            .Where(c => requested.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missing = requested.Except(existing).ToList();
        if (missing.Count > 0)
        {
            return Error.Conflict(
                "Classrooms.UnknownCourses",
                $"Unknown course ids: {string.Join(", ", missing)}");
        }

        classroom.Courses.Clear();
        foreach (var courseId in requested)
        {
            classroom.Courses.Add(new Domain.Entities.Delivery.ClassroomCourse(classroom.Id, courseId, classroom.Courses.Count));
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
