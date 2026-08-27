using ErrorOr;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Messaging.Decorators;

/// <summary>
/// Innermost middleware: wraps command execution in a unit-of-work transaction.
/// Queries are never decorated. Nested sends reuse the ambient transaction.
/// </summary>
internal sealed class TransactionCommandDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async ValueTask<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            return await inner.Handle(command, cancellationToken);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await inner.Handle(command, cancellationToken);

            if (!result.IsError)
            {
                await unitOfWork.CommitAsync(cancellationToken);
            }
            else
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

internal sealed class TransactionCommandDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IUnitOfWork unitOfWork) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async ValueTask<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (unitOfWork.HasActiveTransaction)
        {
            return await inner.Handle(command, cancellationToken);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await inner.Handle(command, cancellationToken);

            if (!result.IsError)
            {
                await unitOfWork.CommitAsync(cancellationToken);
            }
            else
            {
                await unitOfWork.RollbackAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
