using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess.Auditing;
using SmartLMS.DataAccess.Repositories;

namespace SmartLMS.DataAccess;

public static class DependencyInjection
{
    public const string ConnectionStringName = "SmartLMS";

    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<OutboxSaveChangesInterceptor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxMessageRepository, Repositories.OutboxMessageRepository>();

        services.AddDbContext<SmartLmsDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddScoped<ISmartLmsDbContext>(sp => sp.GetRequiredService<SmartLmsDbContext>());
        services.AddScoped<IDbSeeder, Seed.DbSeeder>();

        return services;
    }
}
