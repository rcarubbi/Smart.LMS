namespace SmartLMS.Domain.Events;

/// <summary>
/// Marker for a domain event captured by an aggregate and dispatched to the
/// integration (outbox) layer on <c>SaveChanges</c>.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }

    /// <summary>Business correlation key the outbox persists with the event
    /// (e.g. the plan that raised it), or null when the event has none.</summary>
    Guid? CorrelationId { get; }
}
