using SmartLMS.Domain.Events;

namespace SmartLMS.Domain.Entities;

/// <summary>
/// Base class for aggregates that capture domain events. Events are recorded
/// during a command and flushed to the outbox by the persistence interceptor
/// in the same <c>SaveChanges</c> that commits the aggregate change.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Unraised domain events captured by this aggregate since the last flush.</summary>
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents;

    /// <summary>Records an event for later outbox dispatch on the next SaveChanges.</summary>
    public void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears recorded events after they have been persisted to the outbox.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
