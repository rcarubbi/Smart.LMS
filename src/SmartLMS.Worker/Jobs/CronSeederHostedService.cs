using Microsoft.EntityFrameworkCore;
using SmartLMS.Worker.TickerQPersistence;

namespace SmartLMS.Worker.Jobs;

/// <summary>Registers the hourly ContentDelivery cron ticker if it does not exist yet.</summary>
public class CronSeederHostedService(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartLmsTickerDbContext>();

        var exists = await context.CronTickers
            .AnyAsync(t => t.Function == "ContentDelivery", cancellationToken);

        if (exists)
        {
            return;
        }

        context.CronTickers.Add(new CronTicker
        {
            Function = "ContentDelivery",
            Expression = "0 * * * *",
            Description = "Releases due classes from delivery plans (hourly)",
            Retries = 3,
            RetryIntervals = [60, 300, 600],
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
