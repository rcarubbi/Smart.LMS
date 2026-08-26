using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Messaging;
using SmartLMS.Application.Messaging.Decorators;

namespace SmartLMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddScoped<IDispatcher, Dispatcher>();

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

        // Cross-cutting middleware. Each Decorate call wraps the previous layer,
        // so registration order builds the pipeline inside-out:
        // Logging(Validation(Transaction(inner))).
        services.Decorate(typeof(ICommandHandler<>), typeof(TransactionCommandDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionCommandDecorator<,>));

        services.Decorate(typeof(ICommandHandler<>), typeof(ValidationCommandDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandDecorator<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationQueryDecorator<,>));

        services.Decorate(typeof(ICommandHandler<>), typeof(LoggingCommandDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingCommandDecorator<,>));
        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingQueryDecorator<,>));

        return services;
    }
}
