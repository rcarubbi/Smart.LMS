using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.Infrastructure.Security;

namespace SmartLMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // Mailer, JWT and storage adapters land in their respective stages.
        return services;
    }
}
