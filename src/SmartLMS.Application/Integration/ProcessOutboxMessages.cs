using Microsoft.Extensions.Logging;
using ErrorOr;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Integration;

/// <summary>
/// Drains a batch of unprocessed outbox messages, dispatching each to its
/// handling strategy (<see cref="IOutboxEventHandler"/> resolved by message type).
/// Returns the number of messages processed. Idempotent per message via the
/// outbox processed flag.
/// </summary>
/// <summary>Owns its transaction: SMTP fan-out must not hold the decorator's
/// ambient DB transaction open while calling external mail servers.</summary>
public sealed record ProcessOutboxMessagesCommand : ICommand<int>, IOwnsTransaction;

internal sealed class ProcessOutboxMessagesHandler(
    ISmartLmsDbContext context,
    IOutboxMessageRepository outbox,
    IEnumerable<IOutboxEventHandler> handlers,
    TimeProvider timeProvider,
    ILogger<ProcessOutboxMessagesHandler> logger) : ICommandHandler<ProcessOutboxMessagesCommand, int>
{
    private const int BatchSize = 50;

    /// <summary>
    /// After this many failed attempts a message is skipped (dead-letter by omission):
    /// the repository filters it out so one poison message can't block the queue head.
    /// </summary>
    private const int MaxAttempts = 5;

    private readonly Dictionary<string, IOutboxEventHandler> _handlers = handlers.ToDictionary(h => h.MessageType);

    public async ValueTask<ErrorOr<int>> Handle(
        ProcessOutboxMessagesCommand command,
        CancellationToken cancellationToken)
    {
        var messages = await outbox.GetUnprocessedAsync(BatchSize, MaxAttempts, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var processed = 0;

        foreach (var message in messages)
        {
            try
            {
                if (!_handlers.TryGetValue(message.Type, out var handler))
                {
                    message.MarkFailed($"Unhandled outbox type '{message.Type}'.");
                    logger.LogWarning("Unknown outbox message type {Type} (id {Id})", message.Type, message.Id);
                    continue;
                }

                await handler.HandleAsync(message.Payload, cancellationToken);

                message.MarkProcessed(now);
                processed++;
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);
                logger.LogError(ex, "Failed to process outbox message {Id} ({Type})", message.Id, message.Type);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return processed;
    }
}
