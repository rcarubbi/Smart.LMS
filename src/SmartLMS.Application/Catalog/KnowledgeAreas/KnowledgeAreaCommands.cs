using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

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
    public async ValueTask<Result<Guid>> Handle(CreateKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var duplicated = await context.KnowledgeAreas
            .AnyAsync(k => k.Name == command.Name, cancellationToken);

        if (duplicated)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "KnowledgeAreas.NameAlreadyExists",
                $"A knowledge area named '{command.Name}' already exists."));
        }

        var area = new Domain.Entities.Content.KnowledgeArea { Name = command.Name, Order = command.Order, Active = true };
        context.KnowledgeAreas.Add(area);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(area.Id);
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
    public async ValueTask<Result> Handle(UpdateKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Result.Failure(Error.NotFound("KnowledgeAreas.NotFound", "Knowledge area not found."));
        }

        area.Name = command.Name;
        area.Order = command.Order;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record DeleteKnowledgeAreaCommand(Guid Id) : ICommand;

internal sealed class DeleteKnowledgeAreaHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteKnowledgeAreaCommand>
{
    public async ValueTask<Result> Handle(DeleteKnowledgeAreaCommand command, CancellationToken cancellationToken)
    {
        var area = await context.KnowledgeAreas.FindAsync([command.Id], cancellationToken);
        if (area is null)
        {
            return Result.Failure(Error.NotFound("KnowledgeAreas.NotFound", "Knowledge area not found."));
        }

        var hasSubjects = await context.Subjects.AnyAsync(s => s.KnowledgeAreaId == command.Id, cancellationToken);
        if (hasSubjects)
        {
            return Result.Failure(Error.Conflict(
                "KnowledgeAreas.HasSubjects",
                "Cannot delete a knowledge area that still has subjects."));
        }

        context.KnowledgeAreas.Remove(area);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// ----- Queries -----

public sealed record ListKnowledgeAreasQuery : IQuery<IReadOnlyList<CatalogListItem>>;

internal sealed class ListKnowledgeAreasHandler(ISmartLmsDbContext context)
    : IQueryHandler<ListKnowledgeAreasQuery, IReadOnlyList<CatalogListItem>>
{
    public async ValueTask<Result<IReadOnlyList<CatalogListItem>>> Handle(
        ListKnowledgeAreasQuery query,
        CancellationToken cancellationToken)
    {
        var items = await context.KnowledgeAreas
            .OrderBy(k => k.Order).ThenBy(k => k.Name)
            .Select(k => new CatalogListItem(k.Id, k.Name, k.Order))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CatalogListItem>>(items);
    }
}
