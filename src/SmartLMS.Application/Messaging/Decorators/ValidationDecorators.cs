using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Messaging.Decorators;

/// <summary>
/// Runs every registered FluentValidation validator for the incoming message.
/// Any failure short-circuits with a ValidationError before the handler runs.
/// Validators are resolved from IServiceProvider so the open-generic decoration
/// remains compatible with the DI container's build-time validation.
/// </summary>
internal sealed class ValidationCommandDecorator<TCommand>(
    IServiceProvider serviceProvider,
    ICommandHandler<TCommand> inner) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async ValueTask<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(command, serviceProvider.GetServices<IValidator<TCommand>>(), cancellationToken);
        return failures.Count > 0
            ? Error.Validation("ValidationFailed", "One or more validation errors occurred.", new Dictionary<string, object> { ["ValidationErrors"] = failures })
            : await inner.Handle(command, cancellationToken);
    }
}

internal sealed class ValidationCommandDecorator<TCommand, TResponse>(
    IServiceProvider serviceProvider,
    ICommandHandler<TCommand, TResponse> inner) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async ValueTask<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(command, serviceProvider.GetServices<IValidator<TCommand>>(), cancellationToken);
        return failures.Count > 0
            ? Error.Validation("ValidationFailed", "One or more validation errors occurred.", new Dictionary<string, object> { ["ValidationErrors"] = failures })
            : await inner.Handle(command, cancellationToken);
    }
}

internal sealed class ValidationQueryDecorator<TQuery, TResponse>(
    IServiceProvider serviceProvider,
    IQueryHandler<TQuery, TResponse> inner) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async ValueTask<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var failures = await ValidationRunner.ValidateAsync(query, serviceProvider.GetServices<IValidator<TQuery>>(), cancellationToken);
        return failures.Count > 0
            ? Error.Validation("ValidationFailed", "One or more validation errors occurred.", new Dictionary<string, object> { ["ValidationErrors"] = failures })
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
