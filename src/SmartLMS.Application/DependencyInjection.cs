using Microsoft.Extensions.DependencyInjection;

namespace SmartLMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // CQRS handlers + decorators (Scrutor) are wired up in the CQRS core stage.
        return services;
    }
}
