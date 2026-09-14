using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Integration;
using SmartLMS.Domain.Events;
using SmartLMS.Domain.Events.Delivery;

namespace SmartLMS.DataAccess.Auditing;

/// <summary>
/// Drains domain events captured by <see cref="AggregateRoot"/> aggregates into
/// <see cref="OutboxMessage"/> rows in the same SaveChanges that commits the
/// aggregate change, guaranteeing the event is never lost on a failed commit.
/// </summary>
public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            EnqueueDomainEvents(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            EnqueueDomainEvents(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void EnqueueDomainEvents(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<AggregateRoot>().ToList())
        {
            var aggregate = entry.Entity;
            var events = aggregate.GetDomainEvents();
            if (events.Count == 0)
            {
                continue;
            }

            foreach (var domainEvent in events)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage(
                    domainEvent.GetType().Name,
                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
                    correlationId: GetCorrelationId(domainEvent)));
            }

            aggregate.ClearDomainEvents();
        }
    }

    private static Guid? GetCorrelationId(IDomainEvent domainEvent) => domainEvent switch
    {
        ClassDeliveredDomainEvent delivered => delivered.PlanId,
        _ => null,
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
