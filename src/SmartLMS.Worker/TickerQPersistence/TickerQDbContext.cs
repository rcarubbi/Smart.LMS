using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Entities;

namespace SmartLMS.Worker.TickerQPersistence;

/// <summary>
/// TickerQ persistence context. Uses the built-in entities intentionally:
/// TickerQ.Dashboard (10.4.0) casts cron occurrences to
/// <c>CronTickerOccurrenceEntity&lt;CronTickerEntity&gt;</c>, which fails for
/// custom subclasses because generic types are invariant (TickerQ#863/#889).
/// </summary>
public sealed class SmartLmsTickerDbContext(DbContextOptions<SmartLmsTickerDbContext> options)
    : TickerQ.EntityFrameworkCore.DbContextFactory.TickerQDbContext<TimeTickerEntity, CronTickerEntity>(options)
{
    public DbSet<TimeTickerEntity> TimeTickers => Set<TimeTickerEntity>();

    public DbSet<CronTickerEntity> CronTickers => Set<CronTickerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Backstop for concurrent worker boot: CronSeeder check-then-add
        // can't duplicate a function, so the same job never runs twice.
        modelBuilder.Entity<CronTickerEntity>()
            .HasIndex(t => t.Function)
            .IsUnique();
    }
}
