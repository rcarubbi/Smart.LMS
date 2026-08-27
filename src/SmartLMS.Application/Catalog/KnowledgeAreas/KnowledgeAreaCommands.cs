using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Content;
using ErrorOr;

namespace SmartLMS.Application.Catalog.KnowledgeAreas;

public sealed record CatalogListItem(Guid Id, string Name, int Order);

// ----- Commands -----

public sealed record CreateKnowledgeAreaCommand(string Name, int Order) : ICommand<Guid>;

internal sealed class CreateKnowledgeAreaValidator : AbstractValidator<CreateKnowledgeAreaCommand>
{
    public CreateKnowledgeAreaValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateKnowledgeAreaHandler(ISmartLmsDbContext context)
    : ICommandHandler<CreateKnowledgeAreaCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.KnowledgeAreas
            .AnyAsync(k => k.Name == command.Name, cancellationToken);

        if (duplicated)
        {
            return Error.Conflict(
                "KnowledgeAreas.NameAlreadyExists",
                $"A knowledge area named '{command.Name}' already exists.");
        }

        var area = new Domain.Entities.Content.KnowledgeArea(command.Name, command.Order);
        context.KnowledgeAreas.Add(area);
        await context.SaveChangesAsync(cancellationToken);

        return area.Id;
    }
}

public sealed record UpdateKnowledgeAreaCommand(Guid Id, string Name, int Order) : ICommand;

internal sealed class UpdateKnowledgeAreaValidator : AbstractValidator<UpdateKnowledgeAreaCommand>
{
    public UpdateKnowledgeAreaValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Order).GreaterThanOrEqualTo(0);
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

        area.Rename(command.Name);
        area.SetOrder(command.Order);
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
            return Error.NotFound("KnowledgeAreas.NotFound", "Knowledge area not found.");
        }

        var hasSubjects = await context.Subjects.AnyAsync(s => s.KnowledgeAreaId == command.Id, cancellationToken);
        if (hasSubjects)
        {
            return Error.Conflict(
                "KnowledgeAreas.HasSubjects",
                "Cannot delete a knowledge area that still has subjects.");
        }

        context.KnowledgeAreas.Remove(area);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}

// ----- Queries -----

public sealed record ListKnowledgeAreasQuery : IQuery<IReadOnlyList<CatalogListItem>>;

internal sealed class ListKnowledgeAreasHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListKnowledgeAreasQuery, IReadOnlyList<CatalogListItem>>
{
    public async ValueTask<ErrorOr<IReadOnlyList<CatalogListItem>>> Handle(
        ListKnowledgeAreasQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.KnowledgeAreas
            .OrderBy(k => k.Order).ThenBy(k => k.Name)
            .Select(k => new CatalogListItem(k.Id, k.Name, k.Order))
            .ToListAsync(cancellationToken);

        return items;
    }
}
