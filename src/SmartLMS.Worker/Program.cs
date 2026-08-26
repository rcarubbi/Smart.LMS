using Microsoft.EntityFrameworkCore;
using SmartLMS.Application;
using SmartLMS.DataAccess;
using SmartLMS.Infrastructure;
using SmartLMS.Worker.Jobs;
using SmartLMS.Worker.TickerQPersistence;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Utilities.Models;
using TickerQ.Dashboard.DependencyInjection;
using SmartLMS.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDataAccess(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

// Background deliveries run as the system user (no HTTP context).
builder.Services.AddSingleton<ICurrentUserProvider, SystemCurrentUserProvider>();
builder.Services.AddSingleton(TimeProvider.System);

var tickerConnectionString = builder.Configuration
    .GetConnectionString(SmartLMS.DataAccess.DependencyInjection.ConnectionStringName)
    ?? throw new InvalidOperationException("Connection string 'SmartLMS' is not configured.");

builder.Services.AddDbContext<SmartLmsTickerDbContext>(options => options.UseSqlServer(tickerConnectionString));

builder.Services.AddTickerQ<TimeTicker, CronTicker>(options =>
{
    options.AddOperationalStore(ef =>
        ef.UseTickerQDbContext<SmartLmsTickerDbContext>(dbOptions => dbOptions.UseSqlServer(tickerConnectionString)));

    options.AddDashboard(dashboard =>
    {
        dashboard.SetBasePath("/tickerq");
        dashboard.WithBasicAuth(
            builder.Configuration["TickerQ:Dashboard:Username"] ?? "admin",
            builder.Configuration["TickerQ:Dashboard:Password"] ?? "admin");
    });
});

// Seed the hourly cron ticker on first run (dashboard can manage it afterwards).
builder.Services.AddHostedService<CronSeederHostedService>();

var app = builder.Build();

await MigrateAsync(app.Services);

app.UseTickerQ();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

await app.RunAsync();

static async Task MigrateAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();

    var tickerContext = scope.ServiceProvider.GetRequiredService<SmartLmsTickerDbContext>();
    await tickerContext.Database.MigrateAsync();

    var mainContext = scope.ServiceProvider.GetRequiredService<SmartLmsDbContext>();
    await mainContext.Database.MigrateAsync();
}
