using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Catalog.Courses;
using ErrorOr;
using SmartLMS.Domain.Entities.Content;
using ClassSummary = SmartLMS.Application.Catalog.Courses.ClassSummary;

namespace SmartLMS.Application.Catalog.Classes;

public sealed record CreateClassCommand(
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    ContentType ContentType,
    Guid CourseId,
    Guid TeacherId) : ICommand<Guid>;

internal sealed class CreateClassValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Content).NotEmpty();
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DeliveryDays).GreaterThanOrEqualTo(0);
        RuleFor(c => c.ContentType).IsInEnum();
        RuleFor(c => c.CourseId).NotEmpty();
        RuleFor(c => c.TeacherId).NotEmpty();
    }
}

internal sealed class CreateClassHandler(ISmartLmsDbContext context) : ICommandHandler<CreateClassCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == command.CourseId, cancellationToken);
        if (!courseExists)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var teacherIsTeacher = await context.Users
            .AnyAsync(u => u.Id == command.TeacherId && u is Domain.Entities.UserAccess.Teacher, cancellationToken);

        if (!teacherIsTeacher)
        {
            return Error.Conflict("Classes.InvalidTeacher", "The informed teacher does not exist or is not a teacher.");
        }

        var klass = new Class(
            command.Name,
            command.Content,
            command.Order,
            command.DeliveryDays,
            command.ContentType,
            command.CourseId,
            command.TeacherId);

        context.Classes.Add(klass);
        await context.SaveChangesAsync(cancellationToken);

        return klass.Id;
    }
}

public sealed record UpdateClassCommand(
    Guid Id,
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    ContentType ContentType,
    Guid TeacherId) : ICommand;

internal sealed class UpdateClassValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Content).NotEmpty();
    }
}

internal sealed class UpdateClassHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateClassCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var klass = await context.Classes.FindAsync([command.Id], cancellationToken);
        if (klass is null)
        {
            return Error.NotFound("Classes.NotFound", "Class not found.");
        }

        klass.Rename(command.Name);
        klass.ChangeContent(command.Content);
        klass.SetOrder(command.Order);
        klass.ChangeDeliveryDays(command.DeliveryDays);
        klass.SetContentType(command.ContentType);
        klass.AssignTeacher(command.TeacherId);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteClassCommand(Guid Id) : ICommand;

internal sealed class DeleteClassHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteClassCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var klass = await context.Classes.FindAsync([command.Id], cancellationToken);
        if (klass is null)
        {
            return Error.NotFound("Classes.NotFound", "Class not found.");
        }

        var hasHistory = await context.ClassAccesses.AnyAsync(a => a.ClassId == command.Id, cancellationToken)
                         || await context.ClassDeliveryPlans.AnyAsync(p => p.ClassId == command.Id, cancellationToken);

        if (hasHistory)
        {
            return Error.Conflict(
                "Classes.HasHistory",
                "Cannot delete a class that already has access history or deliveries.");
        }

        context.Classes.Remove(klass);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record ClassDetail(
    Guid Id,
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    string ContentType,
    Guid CourseId,
    string CourseName,
    Guid TeacherId,
    IReadOnlyList<FileSummary> Files);

public sealed record FileSummary(Guid Id, string Name, string StoragePath);

public sealed record GetClassQuery(Guid Id) : IQuery<ClassDetail>;

internal sealed class GetClassHandler(ISmartLmsDbContext context) : IQueryHandler<GetClassQuery, ClassDetail>
{
    public async ValueTask<ErrorOr<ClassDetail>> Handle(GetClassQuery query, CancellationToken cancellationToken)
    {
        var detail = await context.Classes
            .Where(c => c.Id == query.Id)
            .Select(c => new ClassDetail(
                c.Id,
                c.Name,
                c.Content,
                c.Order,
                c.DeliveryDays,
                c.ContentType.ToString(),
                c.CourseId,
                c.Course.Name,
                c.TeacherId,
                c.Files.OrderBy(f => f.Name).Select(f => new FileSummary(f.Id, f.Name, f.StoragePath)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null
            ? Error.NotFound("Classes.NotFound", "Class not found.")
            : detail;
    }
}

public sealed record ListClassesQuery(Guid CourseId) : IQuery<IReadOnlyList<ClassSummary>>;

internal sealed class ListClassesHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListClassesQuery, IReadOnlyList<ClassSummary>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<ClassSummary>>> Handle(
        ListClassesQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Classes
            .Where(c => c.CourseId == query.CourseId)
            .OrderBy(c => c.Order).ThenBy(c => c.Name)
            .Select(c => new Courses.ClassSummary(c.Id, c.Name, c.Order, c.DeliveryDays, c.ContentType.ToString()))
            .ToListAsync(cancellationToken);

        return items;
    }
}
