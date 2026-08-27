using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Catalog.KnowledgeAreas;
using ErrorOr;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Application.Catalog.Subjects;

public sealed record CreateSubjectCommand(string Name, int Order, Guid KnowledgeAreaId) : ICommand<Guid>;

internal sealed class CreateSubjectValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
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

        var subject = new Subject(command.Name, command.Order, command.KnowledgeAreaId);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync(cancellationToken);

        return subject.Id;
    }
}

public sealed record UpdateSubjectCommand(Guid Id, string Name, int Order) : ICommand;

internal sealed class UpdateSubjectValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateSubjectHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateSubjectCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateSubjectCommand command, CancellationToken cancellationToken)
    {
        var subject = await context.Subjects.FindAsync([command.Id], cancellationToken);
        if (subject is null)
        {
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        subject.Rename(command.Name);
        subject.SetOrder(command.Order);
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
            return Error.NotFound("Subjects.NotFound", "Subject not found.");
        }

        var hasCourses = await context.Courses.AnyAsync(c => c.SubjectId == command.Id, cancellationToken);
        if (hasCourses)
        {
            return Error.Conflict("Subjects.HasCourses", "Cannot delete a subject that still has courses.");
        }

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record ListSubjectsQuery(Guid? KnowledgeAreaId) : IQuery<IReadOnlyList<CatalogListItem>>;

internal sealed class ListSubjectsHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListSubjectsQuery, IReadOnlyList<CatalogListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<CatalogListItem>>> Handle(
        ListSubjectsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.Subjects
            .Where(s => query.KnowledgeAreaId == null || s.KnowledgeAreaId == query.KnowledgeAreaId)
            .OrderBy(s => s.Order).ThenBy(s => s.Name)
            .Select(s => new CatalogListItem(s.Id, s.Name, s.Order))
            .ToListAsync(cancellationToken);

        return items;
    }
}
