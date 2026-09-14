using Microsoft.EntityFrameworkCore;
using SmartLMS.Worker.TickerQPersistence;
using TickerQ.Utilities.Entities;

namespace SmartLMS.Worker.Jobs;

/// <summary>Registers the hourly ContentDelivery cron ticker if it does not exist yet.</summary>
public class CronSeederHostedService(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartLmsTickerDbContext>();

        var contentExists = await context.CronTickers
            .AnyAsync(t => t.Function == "ContentDelivery", cancellationToken);

        if (!contentExists)
        {
            context.CronTickers.Add(new CronTickerEntity
            {
                Function = "ContentDelivery",
                Expression = "0 * * * *",
                Description = "Releases due classes from delivery plans (hourly)",
                Retries = 3,
                RetryIntervals = [60, 300, 600],
            });
        }

        var outboxExists = await context.CronTickers
            .AnyAsync(t => t.Function == "OutboxProcessor", cancellationToken);

        if (!outboxExists)
        {
            context.CronTickers.Add(new CronTickerEntity
            {
                Function = "OutboxProcessor",
                Expression = "* * * * *",
                Description = "Drains the integration outbox (every minute)",
                Retries = 3,
                RetryIntervals = [60, 300, 600],
            });
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Benign: another worker instance seeded concurrently and won
            // the unique Function race. Our read model is already correct.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
