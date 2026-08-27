using System.Collections.Concurrent;
using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Messaging;

/// <summary>
/// Runtime dispatch over closed handler interfaces.
/// A small wrapper object per concrete message type is cached once, so steady-state
/// dispatch costs one dictionary lookup plus an interface call.
/// </summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, CommandHandlerWrapper> CommandWrappers = new();
    private static readonly ConcurrentDictionary<Type, ResponseHandlerWrapperBase> ResponseWrappers = new();

    public ValueTask<ErrorOr<Success>> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var wrapper = CommandWrappers.GetOrAdd(
            command.GetType(),
            static messageType => (CommandHandlerWrapper)Activator.CreateInstance(
                typeof(CommandHandlerWrapperImpl<>).MakeGenericType(messageType))!);

        return wrapper.Handle(command, serviceProvider, cancellationToken);
    }

    public ValueTask<ErrorOr<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var resultTask = InvokeResponse(command, cancellationToken);
        return Wrap<TResponse>(resultTask);
    }

    public ValueTask<ErrorOr<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var resultTask = InvokeResponse(query, cancellationToken);
        return Wrap<TResponse>(resultTask);
    }

    private async ValueTask<object> InvokeResponse(object message, CancellationToken cancellationToken)
    {
        var wrapper = ResponseWrappers.GetOrAdd(message.GetType(), CreateResponseWrapper);

        return await wrapper.Handle(message, serviceProvider, cancellationToken);
    }

    private static Type GetResponseType(Type messageType) =>
        messageType.GetInterfaces()
            .First(i => i.IsGenericType &&
                        (i.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                         i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .GetGenericArguments()[0];

    private static ResponseHandlerWrapperBase CreateResponseWrapper(Type messageType)
    {
        var responseType = GetResponseType(messageType);
        var isQuery = messageType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

        var wrapperType = isQuery
            ? typeof(QueryHandlerWrapperImpl<,>).MakeGenericType(messageType, responseType)
            : typeof(CommandResponseHandlerWrapperImpl<,>).MakeGenericType(messageType, responseType);

        return (ResponseHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;
    }

    private static async ValueTask<ErrorOr<TResponse>> Wrap<TResponse>(ValueTask<object> task)
    {
        var result = await task;
        return (ErrorOr<TResponse>)result;
    }
}

internal abstract class CommandHandlerWrapper
{
    public abstract ValueTask<ErrorOr<Success>> Handle(object message, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class CommandHandlerWrapperImpl<TCommand> : CommandHandlerWrapper
    where TCommand : ICommand
{
    public override ValueTask<ErrorOr<Success>> Handle(object message, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        serviceProvider
            .GetRequiredService<ICommandHandler<TCommand>>()
            .Handle((TCommand)message, cancellationToken);
}

internal abstract class ResponseHandlerWrapperBase
{
    public abstract ValueTask<object> Handle(object message, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class CommandResponseHandlerWrapperImpl<TCommand, TResponse> : ResponseHandlerWrapperBase
    where TCommand : ICommand<TResponse>
{
    public override async ValueTask<object> Handle(object message, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await serviceProvider
            .GetRequiredService<ICommandHandler<TCommand, TResponse>>()
            .Handle((TCommand)message, cancellationToken);
}

internal sealed class QueryHandlerWrapperImpl<TQuery, TResponse> : ResponseHandlerWrapperBase
    where TQuery : IQuery<TResponse>
{
    public override async ValueTask<object> Handle(object message, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await serviceProvider
            .GetRequiredService<IQueryHandler<TQuery, TResponse>>()
            .Handle((TQuery)message, cancellationToken);
}
