using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Integration;
using TickerQ.Utilities.Base;

namespace SmartLMS.Worker.Jobs;

/// <summary>
/// Drains the integration outbox: processes unhandled outbox messages
/// (e.g. student delivery e-mails) recorded by domain events on SaveChanges.
/// </summary>
public class OutboxProcessorJobs(IDispatcher dispatcher, ILogger<OutboxProcessorJobs> logger)
{
    [TickerFunction("OutboxProcessor", cronExpression: "* * * * *")]
    public async Task Run(CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new ProcessOutboxMessagesCommand(), cancellationToken);

        if (!result.IsError)
        {
            logger.LogInformation("Outbox processor handled {Count} messages", result.Value);
        }
        else
        {
            logger.LogWarning("Outbox processor failed: {Error}", result.FirstError.Description);
        }
    }
}
