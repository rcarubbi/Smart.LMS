using FluentValidation;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Messaging.Decorators;

/// <summary>
/// Runs every registered FluentValidation validator for the incoming message.
/// Any failure short-circuits with a ValidationError before the handler runs.
/// </summary>
internal sealed class ValidationCommandDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async ValueTask<Result> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(command, validators, cancellationToken);
        return failures.Count > 0
            ? Result.Failure(new ValidationError(failures))
            : await inner.Handle(command, cancellationToken);
    }
}

internal sealed class ValidationCommandDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async ValueTask<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(command, validators, cancellationToken);
        return failures.Count > 0
            ? Result.Failure<TResponse>(new ValidationError(failures))
            : await inner.Handle(command, cancellationToken);
    }
}

internal sealed class ValidationQueryDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async ValueTask<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(query, validators, cancellationToken);
        return failures.Count > 0
            ? Result.Failure<TResponse>(new ValidationError(failures))
            : await inner.Handle(query, cancellationToken);
    }
}

internal static class ValidationRunner
{
    public static async Task<Dictionary<string, string[]>> ValidateAsync<TRequest>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        CancellationToken cancellationToken)
    {
        var instances = validators as ICollection<IValidator<TRequest>> ?? validators.ToArray();
        if (instances.Count == 0)
        {
            return [];
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(instances.Select(v => v.ValidateAsync(context, cancellationToken)));

        return results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => f.PropertyName, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray(), StringComparer.Ordinal);
    }
}
