using System.Text.Json;
using Microsoft.Extensions.Logging;
using ErrorOr;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Events.Delivery;

namespace SmartLMS.Application.Integration;

/// <summary>
/// Drains a batch of unprocessed outbox messages, dispatching each to its
/// integration side-effect (student delivery e-mails). Returns the number of
/// messages processed. Idempotent per message via the outbox processed flag.
/// </summary>
/// <summary>Owns its transaction: SMTP fan-out must not hold the decorator's
/// ambient DB transaction open while calling external mail servers.</summary>
public sealed record ProcessOutboxMessagesCommand : ICommand<int>, IOwnsTransaction;

internal sealed class ProcessOutboxMessagesHandler(
    ISmartLmsDbContext context,
    IOutboxMessageRepository outbox,
    IEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<ProcessOutboxMessagesHandler> logger) : ICommandHandler<ProcessOutboxMessagesCommand, int>
{
    private const int BatchSize = 50;

    /// <summary>
    /// After this many failed attempts a message is skipped (dead-letter by omission):
    /// the repository filters it out so one poison message can't block the queue head.
    /// Must match the serializer below with the interceptor's camelCase payload.
    /// </summary>
    private const int MaxAttempts = 5;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

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
                switch (message.Type)
                {
                    case nameof(ClassDeliveredDomainEvent):
                        await DeliverClassNotification(message.Payload, cancellationToken);
                        break;
                    default:
                        message.MarkFailed($"Unhandled outbox type '{message.Type}'.");
                        logger.LogWarning("Unknown outbox message type {Type} (id {Id})", message.Type, message.Id);
                        continue;
                }

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

    private async Task DeliverClassNotification(string payload, CancellationToken cancellationToken)
    {
        var evt = JsonSerializer.Deserialize<ClassDeliveredDomainEvent>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize ClassDeliveredDomainEvent payload.");

        foreach (var recipient in evt.Recipients)
        {
            await emailSender.SendAsync(
                recipient.Email,
                $"New class available: {evt.ClassName}",
                $"Hello {recipient.Name},<br/>The class <b>{evt.ClassName}</b> of course <b>{evt.CourseName}</b> ({evt.SubjectName}) has just been released in classroom {evt.ClassroomName}.",
                cancellationToken);
        }
    }
}
