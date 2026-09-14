namespace SmartLMS.Application.Integration;

/// <summary>
/// Strategy for one outbox message type (one domain-event integration side-effect).
/// Handlers are looked up by <see cref="MessageType"/> at drain time, so adding a new
/// event means adding a handler class, not editing a dispatch switch.
/// </summary>
public interface IOutboxEventHandler
{
    /// <summary>Matches <see cref="Domain.Events.IDomainEvent"/> subtype names (e.g. <c>nameof</c> of the event).</summary>
    string MessageType { get; }

    ValueTask HandleAsync(string payload, CancellationToken cancellationToken);
}