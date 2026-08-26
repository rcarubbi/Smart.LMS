using SmartLMS.Application.Common;

namespace SmartLMS.Application.Abstractions.Messaging;

/// <summary>A command that produces no return value.</summary>
public interface ICommand;

/// <summary>A command that produces a return value of type <typeparamref name="TResponse"/>.</summary>
public interface ICommand<TResponse>;

/// <summary>A read-only request that never mutates state and runs outside transactions.</summary>
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    ValueTask<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    ValueTask<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    ValueTask<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}

/// <summary>
/// Dispatches commands and queries to their handlers. Handlers are decorated with the
/// cross-cutting middleware pipeline (logging -> validation -> transaction) registered via Scrutor.
/// </summary>
public interface IDispatcher
{
    ValueTask<Result> Send(ICommand command, CancellationToken cancellationToken = default);

    ValueTask<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    ValueTask<Result<TResponse>> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
