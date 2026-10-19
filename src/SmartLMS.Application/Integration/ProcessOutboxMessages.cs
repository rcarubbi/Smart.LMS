using Microsoft.Extensions.Logging;
using ErrorOr;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.Application.Integration;

/// <summary>
/// Drains a claimed batch through its message-type handler. SMTP delivery is at-least-once:
/// a process crash after a send can cause a retry when the lease expires. Owns its transaction
/// so external mail calls do not hold a DB transaction open.
/// </summary>
public sealed record ProcessOutboxMessagesCommand : ICommand<int>, IOwnsTransaction;

internal sealed class ProcessOutboxMessagesHandler(
    ISmartLmsDbContext context,
    IOutboxMessageRepository outbox,
    IEnumerable<IOutboxEventHandler> handlers,
    TimeProvider timeProvider,
    ILogger<ProcessOutboxMessagesHandler> logger) : ICommandHandler<ProcessOutboxMessagesCommand, int>
{
    private const int BatchSize = 50;
    private const int MaxAttempts = OutboxMessage.MaxAttempts;
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);

    private readonly Dictionary<string, IOutboxEventHandler> _handlers = handlers.ToDictionary(h => h.MessageType);

    public async ValueTask<ErrorOr<int>> Handle(
        ProcessOutboxMessagesCommand command,
        CancellationToken cancellationToken)
    {
        var claimedAt = timeProvider.GetUtcNow().UtcDateTime;
        var messages = await outbox.ClaimBatchAsync(BatchSize, MaxAttempts, claimedAt, LeaseDuration, cancellationToken);
        var processed = 0;

        foreach (var message in messages)
        {
            try
            {
                if (!_handlers.TryGetValue(message.Type, out var handler))
                {
                    message.MarkFailed($"Unhandled outbox type '{message.Type}'.", timeProvider.GetUtcNow().UtcDateTime);
                    logger.LogWarning("Unknown outbox message type {Type} (id {Id})", message.Type, message.Id);
                    continue;
                }

                await handler.HandleAsync(message.Payload, cancellationToken);

                message.MarkProcessed(timeProvider.GetUtcNow().UtcDateTime);
                processed++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Leave leases intact so another worker retries the batch after expiry.
                throw;
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message, timeProvider.GetUtcNow().UtcDateTime);
                logger.LogError(ex, "Failed to process outbox message {Id} ({Type})", message.Id, message.Type);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return processed;
    }
}
