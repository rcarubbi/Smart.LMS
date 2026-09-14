using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Messaging.Decorators;

namespace SmartLMS.Application.Messaging;

/// <summary>
/// Rewrites scanned handler registrations into decorator pipelines.
/// Each handler keeps its lifetime; resolving its interface yields
/// Logging(Validation(Transaction(inner))) for commands and
/// Logging(Validation(inner)) for queries.
///
/// Note: this composes decorators through closed-type factories instead of Scrutor's
/// open-generic Decorate(), because the .NET 10 DI engine eagerly populates call sites
/// and rejects open-generic implementations with cross-cutting dependencies.
/// </summary>
internal static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection DecorateMessageHandlers(this IServiceCollection services)
    {
        for (var i = 0; i < services.Count; i++)
        {
            var descriptor = services[i];

            if (!IsHandlerInterface(descriptor.ServiceType))
            {
                continue;
            }

            services[i] = ServiceDescriptor.Describe(
                descriptor.ServiceType,
                serviceProvider =>
                {
                    var inner = CreateInner(descriptor, serviceProvider);
                    return WrapWithPipeline(serviceProvider, descriptor.ServiceType, inner);
                },
                descriptor.Lifetime);
        }

        return services;
    }

    private static bool IsHandlerInterface(Type type) =>
        type.IsGenericType &&
        (type.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
         type.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
         type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

    private static object CreateInner(ServiceDescriptor descriptor, IServiceProvider serviceProvider)
    {
        if (descriptor.ImplementationFactory is { } factory)
        {
            return factory(serviceProvider);
        }

        var implementationType = descriptor.ImplementationType
            ?? throw new InvalidOperationException($"Handler registration for '{descriptor.ServiceType}' has neither implementation type nor factory.");

        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
    }

    private static object WrapWithPipeline(IServiceProvider serviceProvider, Type serviceType, object inner)
    {
        var typeArguments = serviceType.GetGenericArguments();
        var definition = serviceType.GetGenericTypeDefinition();

        // Inside-out composition: transaction -> validation -> logging.
        // IOwnsTransaction commands skip the ambient transaction (they commit per batch).
        return definition switch
        {
            _ when definition == typeof(ICommandHandler<>) =>
                Decorate(typeof(LoggingCommandDecorator<>), typeArguments, serviceProvider,
                    Decorate(typeof(ValidationCommandDecorator<>), typeArguments, serviceProvider,
                        Transact(typeArguments, serviceProvider, inner))),

            _ when definition == typeof(ICommandHandler<,>) =>
                Decorate(typeof(LoggingCommandDecorator<,>), typeArguments, serviceProvider,
                    Decorate(typeof(ValidationCommandDecorator<,>), typeArguments, serviceProvider,
                        Transact(typeArguments, serviceProvider, inner))),

            _ => // IQueryHandler<,>: no transaction wrapper.
                Decorate(typeof(LoggingQueryDecorator<,>), typeArguments, serviceProvider,
                    Decorate(typeof(ValidationQueryDecorator<,>), typeArguments, serviceProvider, inner)),
        };
    }

    private static object Transact(Type[] typeArguments, IServiceProvider serviceProvider, object inner)
    {
        // typeArguments[0] is always TCommand for both ICommandHandler shapes.
        if (typeof(IOwnsTransaction).IsAssignableFrom(typeArguments[0]))
        {
            return inner;
        }

        var openDecorator = typeArguments.Length == 1
            ? typeof(TransactionCommandDecorator<>)
            : typeof(TransactionCommandDecorator<,>);
        return Decorate(openDecorator, typeArguments, serviceProvider, inner);
    }

    private static object Decorate(Type openDecoratorType, Type[] messageTypes, IServiceProvider serviceProvider, object inner)
    {
        var decoratorType = openDecoratorType.MakeGenericType(messageTypes);
        return ActivatorUtilities.CreateInstance(serviceProvider, decoratorType, inner);
    }
}
