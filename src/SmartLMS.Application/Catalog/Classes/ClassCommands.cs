using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Authorization;
using SmartLMS.Application.Catalog.Courses;
using ErrorOr;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using ClassSummary = SmartLMS.Application.Catalog.Courses.ClassSummary;

namespace SmartLMS.Application.Catalog.Classes;

public sealed record SupportFileInput(string Name, string StoragePath);

public sealed record CreateClassInput(
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    ContentType ContentType,
    Guid CourseId,
    Guid TeacherId,
    IReadOnlyList<SupportFileInput>? Files = null);

public sealed record CreateClassCommand(CreateClassInput Input) : ICommand<Guid>;

internal sealed class CreateClassValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Content).NotEmpty();
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.DeliveryDays).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.ContentType).IsInEnum();
        RuleFor(c => c.Input.CourseId).NotEmpty();
        RuleFor(c => c.Input.TeacherId).NotEmpty();
    }
}

internal sealed class CreateClassHandler(ISmartLmsDbContext context) : ICommandHandler<CreateClassCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == command.Input.CourseId, cancellationToken);
        if (!courseExists)
        {
            return Error.NotFound("Courses.NotFound", "Course not found.");
        }

        var teacherIsTeacher = await context.Users
            .AnyAsync(u => u.Id == command.Input.TeacherId && u is Domain.Entities.UserAccess.Teacher, cancellationToken);

        if (!teacherIsTeacher)
        {
            return Error.Conflict("Classes.InvalidTeacher", "O professor informado não existe ou não é um professor.");
        }

        var klass = new Class(
            command.Input.Name,
            command.Input.Content,
            command.Input.Order,
            command.Input.DeliveryDays,
            command.Input.ContentType,
            command.Input.CourseId,
            command.Input.TeacherId);

        var files = command.Input.Files ?? [];
        foreach (var file in files)
        {
            klass.Files.Add(new SupportFile(file.Name, file.StoragePath, klass.Id));
        }

        context.Classes.Add(klass);
        await context.SaveChangesAsync(cancellationToken);

        return klass.Id;
    }
}

public sealed record UpdateClassInput(
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    ContentType ContentType,
    Guid TeacherId,
    IReadOnlyList<SupportFileInput>? Files = null);

public sealed record UpdateClassCommand(Guid Id, UpdateClassInput Input) : ICommand;

internal sealed class UpdateClassValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Content).NotEmpty();
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.DeliveryDays).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.ContentType).IsInEnum();
        RuleFor(c => c.Input.TeacherId).NotEmpty();
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

        klass.Rename(command.Input.Name);
        klass.ChangeContent(command.Input.Content);
        klass.SetOrder(command.Input.Order);
        klass.ChangeDeliveryDays(command.Input.DeliveryDays);
        klass.SetContentType(command.Input.ContentType);
        klass.AssignTeacher(command.Input.TeacherId);

        var existing = await context.Files.Where(f => f.ClassId == command.Id).ToListAsync(cancellationToken);
        var incoming = command.Input.Files ?? [];

        foreach (var file in existing.Where(e => incoming.All(i => i.StoragePath != e.StoragePath)).ToList())
        {
            context.Files.Remove(file);
        }

        foreach (var file in incoming.Where(i => existing.All(e => e.StoragePath != i.StoragePath)))
        {
            klass.Files.Add(new SupportFile(file.Name, file.StoragePath, command.Id));
        }

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
            return Error.NotFound("Classes.NotFound", "Aula não encontrada.");
        }

        klass.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreClassCommand(Guid Id) : ICommand;

internal sealed class RestoreClassHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreClassCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreClassCommand command, CancellationToken cancellationToken)
    {
        var klass = await context.Classes.FindAsync([command.Id], cancellationToken);
        if (klass is null)
        {
            return Error.NotFound("Classes.NotFound", "Aula não encontrada.");
        }

        var courseActive = await context.Courses.AnyAsync(c => c.Id == klass.CourseId && c.Active, cancellationToken);
        if (!courseActive)
        {
            return Error.Conflict(
                "Classes.CourseInactive",
                "O curso desta aula está na lixeira. Restaure o curso antes de restaurar a aula.");
        }

        klass.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteClassPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteClassPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteClassPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteClassPermanentlyCommand command, CancellationToken cancellationToken)
    {
        var klass = await context.Classes.FindAsync([command.Id], cancellationToken);
        if (klass is null)
        {
            return Error.NotFound("Classes.NotFound", "Aula não encontrada.");
        }

        var fileIds = await context.Files
            .Where(f => f.ClassId == command.Id)
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        if (fileIds.Count > 0)
        {
            context.FileAccesses.RemoveRange(
                await context.FileAccesses.Where(a => fileIds.Contains(a.FileId)).ToListAsync(cancellationToken));
        }

        context.ClassAccesses.RemoveRange(
            await context.ClassAccesses.Where(a => a.ClassId == command.Id).ToListAsync(cancellationToken));
        context.ClassDeliveryPlans.RemoveRange(
            await context.ClassDeliveryPlans.Where(d => d.ClassId == command.Id).ToListAsync(cancellationToken));
        context.Comments.RemoveRange(
            await context.Comments.Where(c => c.ClassId == command.Id).ToListAsync(cancellationToken));
        context.Files.RemoveRange(
            await context.Files.Where(f => f.ClassId == command.Id).ToListAsync(cancellationToken));

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

internal sealed class GetClassHandler(ClassAccessPolicy accessPolicy)
    : IQueryHandler<GetClassQuery, ClassDetail>
{
    public async ValueTask<ErrorOr<ClassDetail>> Handle(GetClassQuery query, CancellationToken cancellationToken)
    {
        var spec = new ClassByIdSpec(query.Id);
        var detail = await SpecificationEvaluator.Default
            .GetQuery(accessPolicy.AccessibleClasses(), spec)
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

public sealed record ListClassesQuery(
    Guid CourseId,
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<ClassSummary>>;

internal sealed class ListClassesHandler(ClassAccessPolicy accessPolicy)
    : IQueryHandler<ListClassesQuery, Paged<ClassSummary>>
{
    public async ValueTask<ErrorOr<Paged<ClassSummary>>> Handle(
        ListClassesQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new ActiveClassesInCourseSpec(query.CourseId, query.Active);
        var filtered = SpecificationEvaluator.Default.GetQuery(accessPolicy.AccessibleClasses(), spec);

        var items = await filtered
            .Select(c => new Courses.ClassSummary(
                c.Id,
                c.Name,
                c.Order,
                c.DeliveryDays,
                c.ContentType.ToString(),
                c.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
