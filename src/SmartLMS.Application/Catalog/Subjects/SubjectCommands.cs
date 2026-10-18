using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Catalog.Courses;
using ErrorOr;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Catalog.Subjects;

public sealed record SubjectListItem(
    Guid Id,
    string Name,
    int Order,
    Guid KnowledgeAreaId,
    string KnowledgeAreaName,
    bool Active);

public sealed record SubjectInput(string Name, int Order);

public sealed record CreateSubjectCommand(SubjectInput Input, Guid KnowledgeAreaId) : ICommand<Guid>;

internal sealed class CreateSubjectValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.KnowledgeAreaId).NotEmpty();
    }
}

internal sealed class CreateSubjectHandler(ISmartLmsDbContext context) : ICommandHandler<CreateSubjectCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateSubjectCommand command, CancellationToken cancellationToken)
    {
        var areaExists = await context.KnowledgeAreas
            .AnyAsync(k => k.Id == command.KnowledgeAreaId, cancellationToken);

        if (!areaExists)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Knowledge area not found.");
        }

        var subject = new Subject(command.Input.Name, command.Input.Order, command.KnowledgeAreaId);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync(cancellationToken);

        return subject.Id;
    }
}

public sealed record SubjectUpdateInput(string Name, int Order, Guid KnowledgeAreaId);

public sealed record UpdateSubjectCommand(Guid Id, SubjectUpdateInput Input) : ICommand;

internal sealed class UpdateSubjectValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Input.KnowledgeAreaId).NotEmpty();
    }
}

internal sealed class UpdateSubjectHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateSubjectCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateSubjectCommand command, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects.FindAsync([command.Id], cancellationToken);
        if (subject is null)
        {
            return Error.NotFound("Subjects.NotFound", "Assunto não encontrado.");
        }

        var areaExists = await context.KnowledgeAreas
            .AnyAsync(k => k.Id == command.Input.KnowledgeAreaId, cancellationToken);
        if (!areaExists)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Área de conhecimento não encontrada.");
        }

        subject.Rename(command.Input.Name);
        subject.SetOrder(command.Input.Order);
        subject.MoveTo(command.Input.KnowledgeAreaId);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteSubjectCommand(Guid Id) : ICommand;

internal sealed class DeleteSubjectHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteSubjectCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteSubjectCommand command, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects.FindAsync([command.Id], cancellationToken);
        if (subject is null)
        {
            return Error.NotFound("Subjects.NotFound", "Assunto não encontrado.");
        }

        subject.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreSubjectCommand(Guid Id) : ICommand;

internal sealed class RestoreSubjectHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreSubjectCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreSubjectCommand command, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects.FindAsync([command.Id], cancellationToken);
        if (subject is null)
        {
            return Error.NotFound("Subjects.NotFound", "Assunto não encontrado.");
        }

        var areaActive = await context.KnowledgeAreas
            .AnyAsync(k => k.Id == subject.KnowledgeAreaId && k.Active, cancellationToken);
        if (!areaActive)
        {
            return Error.Conflict(
                "Subjects.AreaInactive",
                "A área de conhecimento deste assunto está na lixeira. Restaure a área antes de restaurar o assunto.");
        }

        subject.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteSubjectPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteSubjectPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteSubjectPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteSubjectPermanentlyCommand command, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects.FindAsync([command.Id], cancellationToken);
        if (subject is null)
        {
            return Error.NotFound("Subjects.NotFound", "Assunto não encontrado.");
        }

        var courseIds = await context.Courses
            .Where(c => c.SubjectId == command.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var deleteCourse = new DeleteCoursePermanentlyHandler(context);
        foreach (var courseId in courseIds)
        {
            await deleteCourse.Handle(new DeleteCoursePermanentlyCommand(courseId), cancellationToken);
        }

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record ListSubjectsQuery(
    Guid? KnowledgeAreaId,
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<SubjectListItem>>;

internal sealed class ListSubjectsHandler(ISmartLmsDbContext context, ICurrentUser currentUser)
    : IQueryHandler<ListSubjectsQuery, Paged<SubjectListItem>>
{
    public async ValueTask<ErrorOr<Paged<SubjectListItem>>> Handle(
        ListSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        var active = currentUser.Role == Domain.Entities.UserAccess.UserRole.Admin ? query.Active : true;
        var spec = new SubjectsListSpec(query.KnowledgeAreaId, active);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.Subjects.AsNoTracking(), spec);

        var items = await filtered
            .Select(s => new SubjectListItem(
                s.Id,
                s.Name,
                s.Order,
                s.KnowledgeAreaId,
                s.KnowledgeArea.Name,
                s.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
