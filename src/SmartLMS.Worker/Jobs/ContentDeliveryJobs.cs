using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Learning.Delivery;
using TickerQ.Utilities.Base;

namespace SmartLMS.Worker.Jobs;

/// <summary>
/// Hourly content delivery (port of the legacy Quartz ContentDeliveryJob):
/// releases every due class across open delivery plans, creating notices and
/// notifying enrolled students.
/// </summary>
public class ContentDeliveryJobs(IDispatcher dispatcher, ILogger<ContentDeliveryJobs> logger)
{
    [TickerFunction("ContentDelivery", cronExpression: "0 * * * *")]
    public async Task Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Content delivery job starting");

        var result = await dispatcher.Send(new ExecuteDeliveriesCommand(), cancellationToken);

        if (!result.IsError)
        {
            logger.LogInformation("Content delivery job delivered {Count} classes", result.Value.Count);
        }
        else
        {
            logger.LogWarning("Content delivery job failed: {Error}", result.FirstError.Description);
        }
    }
}
