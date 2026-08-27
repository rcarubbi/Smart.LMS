using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;

namespace SmartLMS.Application.Learning.Classrooms;

public sealed record CreateClassroomCommand(string Name) : ICommand<Guid>;

internal sealed class CreateClassroomValidator : AbstractValidator<CreateClassroomCommand>
{
    public CreateClassroomValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

internal sealed class CreateClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<CreateClassroomCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.Classrooms.AnyAsync(c => c.Name == command.Name, cancellationToken);
        if (duplicated)
        {
            return Error.Conflict("Classrooms.NameAlreadyExists", $"A classroom named '{command.Name}' already exists.");
        }

        var classroom = new Domain.Entities.Delivery.Classroom(command.Name);
        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return classroom.Id;
    }
}

public sealed record UpdateClassroomCommand(Guid Id, string Name) : ICommand;

internal sealed class UpdateClassroomValidator : AbstractValidator<UpdateClassroomCommand>
{
    public UpdateClassroomValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
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

        classroom.Rename(command.Name);
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

        var hasPlans = await context.DeliveryPlans.AnyAsync(p => p.ClassroomId == command.Id, cancellationToken);
        if (hasPlans)
        {
            return Error.Conflict("Classrooms.HasDeliveryPlans", "Cannot delete a classroom that has delivery plans.");
        }

        context.Classrooms.Remove(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record ClassroomListItem(Guid Id, string Name, int CourseCount, int PlanCount);

public sealed record ListClassroomsQuery : IQuery<IReadOnlyList<ClassroomListItem>>;

internal sealed class ListClassroomsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListClassroomsQuery, IReadOnlyList<ClassroomListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<ClassroomListItem>>> Handle(
        ListClassroomsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Classrooms
            .OrderBy(c => c.Name)
            .Select(c => new ClassroomListItem(c.Id, c.Name, c.Courses.Count, c.DeliveryPlans.Count))
            .ToListAsync(cancellationToken);

        return items;
    }
}

/// <summary>Replaces the ordered course list of a classroom.</summary>
public sealed record EnrollClassroomCoursesCommand(Guid ClassroomId, IReadOnlyList<Guid> CourseIds) : ICommand;

internal sealed class EnrollClassroomCoursesHandler(ISmartLmsDbContext context)
    : ICommandHandler<EnrollClassroomCoursesCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(EnrollClassroomCoursesCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .Include(c => c.Courses)
            .SingleOrDefaultAsync(c => c.Id == command.ClassroomId, cancellationToken);

        if (classroom is null)
        {
            return Error.NotFound("Classrooms.NotFound", "Classroom not found.");
        }

        var requested = command.CourseIds.Distinct().ToList();
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
