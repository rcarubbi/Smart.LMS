using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Catalog.Subjects;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Specifications;
using Ardalis.Specification.EntityFrameworkCore;
using ErrorOr;

namespace SmartLMS.Application.Catalog.KnowledgeAreas;

public sealed record CatalogListItem(Guid Id, string Name, int Order, bool Active);

// ----- Commands -----

public sealed record KnowledgeAreaInput(string Name, int Order);

public sealed record CreateKnowledgeAreaCommand(KnowledgeAreaInput Input) : ICommand<Guid>;

internal sealed class CreateKnowledgeAreaValidator : AbstractValidator<CreateKnowledgeAreaCommand>
{
    public CreateKnowledgeAreaValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateKnowledgeAreaHandler(ISmartLmsDbContext context)
    : ICommandHandler<CreateKnowledgeAreaCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.KnowledgeAreas
            .AnyAsync(k => k.Name == command.Input.Name, cancellationToken);

        if (duplicated)
        {
            return Error.Conflict(
                "KnowledgeAreas.NameAlreadyExists",
                $"A knowledge area named '{command.Input.Name}' already exists.");
        }

        var area = new Domain.Entities.Content.KnowledgeArea(command.Input.Name, command.Input.Order);
        context.KnowledgeAreas.Add(area);
        await context.SaveChangesAsync(cancellationToken);

        return area.Id;
    }
}

public sealed record UpdateKnowledgeAreaCommand(Guid Id, KnowledgeAreaInput Input) : ICommand;

internal sealed class UpdateKnowledgeAreaValidator : AbstractValidator<UpdateKnowledgeAreaCommand>
{
    public UpdateKnowledgeAreaValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Order).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateKnowledgeAreaHandler(ISmartLmsDbContext context) : ICommandHandler<UpdateKnowledgeAreaCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Knowledge area not found.");
        }

        area.Rename(command.Input.Name);
        area.SetOrder(command.Input.Order);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteKnowledgeAreaCommand(Guid Id) : ICommand;

internal sealed class DeleteKnowledgeAreaHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteKnowledgeAreaCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Área de conhecimento não encontrada.");
        }

        area.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record RestoreKnowledgeAreaCommand(Guid Id) : ICommand;

internal sealed class RestoreKnowledgeAreaHandler(ISmartLmsDbContext context)
    : ICommandHandler<RestoreKnowledgeAreaCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Área de conhecimento não encontrada.");
        }

        area.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

public sealed record DeleteKnowledgeAreaPermanentlyCommand(Guid Id) : ICommand;

internal sealed class DeleteKnowledgeAreaPermanentlyHandler(ISmartLmsDbContext context)
    : ICommandHandler<DeleteKnowledgeAreaPermanentlyCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(
        DeleteKnowledgeAreaPermanentlyCommand command,
        CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Error.NotFound("KnowledgeAreas.NotFound", "Área de conhecimento não encontrada.");
        }

        var subjectIds = await context.Subjects
            .Where(s => s.KnowledgeAreaId == command.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var deleteSubject = new DeleteSubjectPermanentlyHandler(context);
        foreach (var subjectId in subjectIds)
        {
            await deleteSubject.Handle(new DeleteSubjectPermanentlyCommand(subjectId), cancellationToken);
        }

        context.KnowledgeAreas.Remove(area);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

// ----- Queries -----

public sealed record ListKnowledgeAreasQuery(
    bool? Active = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Paged<CatalogListItem>>;

internal sealed class ListKnowledgeAreasHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListKnowledgeAreasQuery, Paged<CatalogListItem>>
{
    public async ValueTask<ErrorOr<Paged<CatalogListItem>>> Handle(
        ListKnowledgeAreasQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new KnowledgeAreasListSpec(query.Active);
        var filtered = SpecificationEvaluator.Default.GetQuery(context.KnowledgeAreas.AsNoTracking(), spec);

        var items = await filtered
            .Select(k => new CatalogListItem(k.Id, k.Name, k.Order, k.Active))
            .PageAsync(query.Page, query.PageSize, cancellationToken);

        return items;
    }
}
