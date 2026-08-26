using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Entities;

namespace SmartLMS.Worker.TickerQPersistence;

/// <summary>TickerQ persistence entities.</summary>
public class TimeTicker : TimeTickerEntity<TimeTicker>
{
}

public class CronTicker : CronTickerEntity
{
}

public sealed class SmartLmsTickerDbContext(DbContextOptions<SmartLmsTickerDbContext> options)
    : TickerQ.EntityFrameworkCore.DbContextFactory.TickerQDbContext<TimeTicker, CronTicker>(options)
{
    public DbSet<TimeTicker> TimeTickers => Set<TimeTicker>();

    public DbSet<CronTicker> CronTickers => Set<CronTicker>();
}
