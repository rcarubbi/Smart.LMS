using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.DataAccess.Auditing;

namespace SmartLMS.DataAccess;

public static class DependencyInjection
{
    public const string ConnectionStringName = "SmartLMS";

    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<SmartLmsDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddScoped<ISmartLmsDbContext>(sp => sp.GetRequiredService<SmartLmsDbContext>());

        return services;
    }
}
