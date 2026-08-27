using System.Diagnostics;
using ErrorOr;
using Microsoft.Extensions.Logging;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Messaging.Decorators;

/// <summary>
/// Outermost middleware: logs every command/query with its duration and outcome.
/// Applied last via Scrutor so it wraps validation and transaction decorators.
/// </summary>
internal sealed class LoggingCommandDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    ILogger<LoggingCommandDecorator<TCommand>> logger) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async ValueTask<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var name = typeof(TCommand).Name;
        var start = Stopwatch.GetTimestamp();

        try
        {
            logger.LogInformation("Handling {MessageName}", name);
            var result = await inner.Handle(command, cancellationToken);
            LogOutcome(name, !result.IsError, start, result.IsError ? result.FirstError.Description : null);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception handling {MessageName} after {ElapsedMs}ms",
                name, ElapsedMs(start));
            throw;
        }
    }

    private void LogOutcome(string name, bool success, long start, string? description) =>
        logger.LogInformation("Handled {MessageName} in {ElapsedMs}ms with {Result}{ErrorSuffix}",
            name, ElapsedMs(start), success ? "success" : "failure",
            success ? string.Empty : $" ({description})");

    private static long ElapsedMs(long start) => Stopwatch.GetElapsedTime(start).Milliseconds;
}

internal sealed class LoggingCommandDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandDecorator<TCommand, TResponse>> logger) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async ValueTask<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var name = typeof(TCommand).Name;
        var start = Stopwatch.GetTimestamp();

        try
        {
            logger.LogInformation("Handling {MessageName}", name);
            var result = await inner.Handle(command, cancellationToken);

            if (result.IsError)
            {
                logger.LogWarning("Handled {MessageName} in {ElapsedMs}ms with failure ({Description})",
                    name, Stopwatch.GetElapsedTime(start).Milliseconds, result.FirstError.Description);
            }
            else
            {
                logger.LogInformation("Handled {MessageName} in {ElapsedMs}ms with success",
                    name, Stopwatch.GetElapsedTime(start).Milliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception handling {MessageName} after {ElapsedMs}ms",
                name, Stopwatch.GetElapsedTime(start).Milliseconds);
            throw;
        }
    }
}

internal sealed class LoggingQueryDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ILogger<LoggingQueryDecorator<TQuery, TResponse>> logger) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async ValueTask<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var name = typeof(TQuery).Name;
        var start = Stopwatch.GetTimestamp();

        try
        {
            logger.LogInformation("Handling {MessageName}", name);
            var result = await inner.Handle(query, cancellationToken);
            logger.LogInformation("Handled {MessageName} in {ElapsedMs}ms with {Result}",
                name, Stopwatch.GetElapsedTime(start).Milliseconds, result.IsError ? "failure" : "success");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception handling {MessageName} after {ElapsedMs}ms",
                name, Stopwatch.GetElapsedTime(start).Milliseconds);
            throw;
        }
    }
}
