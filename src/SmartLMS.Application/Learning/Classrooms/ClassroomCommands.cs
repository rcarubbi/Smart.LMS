using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

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
    public async ValueTask<Result<Guid>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.Classrooms.AnyAsync(c => c.Name == command.Name, cancellationToken);
        if (duplicated)
        {
            return Result.Failure<Guid>(Error.Conflict("Classrooms.NameAlreadyExists", $"A classroom named '{command.Name}' already exists."));
        }

        var classroom = new Domain.Entities.Delivery.Classroom { Name = command.Name, Active = true };
        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(classroom.Id);
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
    public async ValueTask<Result> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Result.Failure(Error.NotFound("Classrooms.NotFound", "Classroom not found."));
        }

        classroom.Name = command.Name;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record DeleteClassroomCommand(Guid Id) : ICommand;

internal sealed class DeleteClassroomHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteClassroomCommand>
{
    public async ValueTask<Result> Handle(DeleteClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms.FindAsync([command.Id], cancellationToken);
        if (classroom is null)
        {
            return Result.Failure(Error.NotFound("Classrooms.NotFound", "Classroom not found."));
        }

        var hasPlans = await context.DeliveryPlans.AnyAsync(p => p.ClassroomId == command.Id, cancellationToken);
        if (hasPlans)
        {
            return Result.Failure(Error.Conflict("Classrooms.HasDeliveryPlans", "Cannot delete a classroom that has delivery plans."));
        }

        context.Classrooms.Remove(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record ClassroomListItem(Guid Id, string Name, int CourseCount, int PlanCount);

public sealed record ListClassroomsQuery : IQuery<IReadOnlyList<ClassroomListItem>>;

internal sealed class ListClassroomsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListClassroomsQuery, IReadOnlyList<ClassroomListItem>>
{
    public async ValueTask<Result<IReadOnlyList<ClassroomListItem>>> Handle(
        ListClassroomsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Classrooms
            .OrderBy(c => c.Name)
            .Select(c => new ClassroomListItem(c.Id, c.Name, c.Courses.Count, c.DeliveryPlans.Count))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClassroomListItem>>(items);
    }
}

/// <summary>Replaces the ordered course list of a classroom.</summary>
public sealed record EnrollClassroomCoursesCommand(Guid ClassroomId, IReadOnlyList<Guid> CourseIds) : ICommand;

internal sealed class EnrollClassroomCoursesHandler(ISmartLmsDbContext context)
    : ICommandHandler<EnrollClassroomCoursesCommand>
{
    public async ValueTask<Result> Handle(EnrollClassroomCoursesCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .Include(c => c.Courses)
            .SingleOrDefaultAsync(c => c.Id == command.ClassroomId, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure(Error.NotFound("Classrooms.NotFound", "Classroom not found."));
        }

        var requested = command.CourseIds.Distinct().ToList();
        var existing = await context.Courses
            .Where(c => requested.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missing = requested.Except(existing).ToList();
        if (missing.Count > 0)
        {
            return Result.Failure(Error.Conflict(
                "Classrooms.UnknownCourses",
                $"Unknown course ids: {string.Join(", ", missing)}"));
        }

        classroom.Courses.Clear();
        foreach (var courseId in requested)
        {
            classroom.Courses.Add(new Domain.Entities.Delivery.ClassroomCourse
            {
                ClassroomId = classroom.Id,
                CourseId = courseId,
                Order = classroom.Courses.Count,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
