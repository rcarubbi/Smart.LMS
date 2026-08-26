using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth;
using SmartLMS.Application.Messaging;

namespace SmartLMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuthTokenIssuer>();

        // Discover all command/query handlers in the application layer.
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>)), publicOnly: false)
            .As(type => type.GetInterfaces().Where(interfaceType =>
                interfaceType.IsGenericType &&
                (interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                 interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                 interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))))
            .WithScopedLifetime());

        // Cross-cutting middleware pipeline (decorator pattern).
        services.DecorateMessageHandlers();

        return services;
    }
}
