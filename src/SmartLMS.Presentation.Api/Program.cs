using Microsoft.EntityFrameworkCore;
using SmartLMS.Application;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess;
using SmartLMS.DataAccess.Auditing;
using SmartLMS.Infrastructure;
using SmartLMS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDataAccess(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await DbSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<SmartLmsDbContext>(),
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
        app.Logger);

    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

await app.RunAsync();

/// <summary>Resolves the audit user from the current HTTP context claims.</summary>
internal sealed class HttpContextCurrentUserProvider(IHttpContextAccessor accessor) : ICurrentUserProvider
{
    public Guid? GetCurrentUserId()
    {
        var value = accessor.HttpContext?.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
