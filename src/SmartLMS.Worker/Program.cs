using Microsoft.EntityFrameworkCore;
using SmartLMS.Application;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess;
using SmartLMS.Infrastructure;
using SmartLMS.Worker.Jobs;
using SmartLMS.Worker.TickerQPersistence;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Models;
using TickerQ.Dashboard.DependencyInjection;
using SmartLMS.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDataAccess(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

// Background deliveries run as the system user (no HTTP context).
// TimeProvider comes from AddApplication above; ICurrentUser covers auditing too.
builder.Services.AddSingleton<ICurrentUser, SystemCurrentUser>();

var tickerConnectionString = builder.Configuration
    .GetConnectionString(SmartLMS.DataAccess.DependencyInjection.ConnectionStringName)
    ?? throw new InvalidOperationException("Connection string 'SmartLMS' is not configured.");
var dashboardUser = builder.Configuration["TickerQ:Dashboard:Username"] ?? (builder.Environment.IsDevelopment() ? "admin" : string.Empty);
var dashboardPassword = builder.Configuration["TickerQ:Dashboard:Password"] ?? (builder.Environment.IsDevelopment() ? "tickerq-admin" : string.Empty);
if (!builder.Environment.IsDevelopment())
{
    var sql = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(tickerConnectionString);
    var signingKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;
    var smtpHost = builder.Configuration["Smtp:Host"];
    if (sql.Password == "yourStrong(!)Password" || signingKey.Length < 32 ||
        signingKey.StartsWith("compose-signing-key-", StringComparison.Ordinal) ||
        dashboardUser is "" or "admin" || dashboardPassword is "" or "admin" or "tickerq-admin" ||
        string.IsNullOrWhiteSpace(smtpHost) || smtpHost is "localhost" or "mailpit")
    {
        throw new InvalidOperationException("Production requires private SQL/JWT secrets and non-default dashboard credentials.");
    }
}

builder.Services.AddDbContext<SmartLmsTickerDbContext>(options => options.UseSqlServer(tickerConnectionString));

builder.Services.AddTickerQ<TimeTickerEntity, CronTickerEntity>(options =>
{
    options.AddOperationalStore(ef =>
        ef.UseTickerQDbContext<SmartLmsTickerDbContext>(dbOptions => dbOptions.UseSqlServer(tickerConnectionString)));

    options.AddDashboard(dashboard =>
    {
        dashboard.SetBasePath("/tickerq");
        dashboard.WithBasicAuth(
            dashboardUser,
            dashboardPassword);
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
