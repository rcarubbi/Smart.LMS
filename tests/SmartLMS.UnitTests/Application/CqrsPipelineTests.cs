using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Common;
using SmartLMS.Application.Messaging;
using SmartLMS.Application.Messaging.Decorators;
using NSubstitute;

namespace SmartLMS.UnitTests.Application;

public class CqrsPipelineTests
{
    [Test]
    public async Task Dispatcher_routes_command_and_query_through_registered_handlers()
    {
        var services = new ServiceCollection();

        var commandHandler = Substitute.For<ICommandHandler<PingCommand>>();
        var commandHandlerWithResponse = Substitute.For<ICommandHandler<EchoCommand, string>>();
        var queryHandler = Substitute.For<IQueryHandler<GetGreetingQuery, string>>();

        commandHandler.Handle(Arg.Any<PingCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success()));

        commandHandlerWithResponse.Handle(Arg.Any<EchoCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success("echo!")));

        queryHandler.Handle(Arg.Any<GetGreetingQuery>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success("hello")));

        services.AddSingleton(commandHandler);
        services.AddSingleton(commandHandlerWithResponse);
        services.AddSingleton(queryHandler);
        services.AddScoped<IDispatcher, Dispatcher>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var pingResult = await dispatcher.Send(new PingCommand());
        var echoResult = await dispatcher.Send(new EchoCommand("ignored"));
        var greetingResult = await dispatcher.Query(new GetGreetingQuery());

        using (Assert.Multiple())
        {
            await Assert.That(pingResult.IsSuccess).IsTrue();
            await Assert.That(echoResult.Value).IsEqualTo("echo!");
            await Assert.That(greetingResult.Value).IsEqualTo("hello");
        }
    }

    [Test]
    public async Task Validation_decorator_short_circuits_with_validation_error()
    {
        var inner = Substitute.For<ICommandHandler<RegisterUserCommand>>();
        var validator = new RegisterUserValidator();

        var provider = new ServiceCollection()
            .AddSingleton<IValidator<RegisterUserCommand>>(validator)
            .BuildServiceProvider();

        var decorator = new ValidationCommandDecorator<RegisterUserCommand>(provider, inner);

        var result = await decorator.Handle(new RegisterUserCommand(Email: "not-an-email"), CancellationToken.None);

        using (Assert.Multiple())
        {
            await Assert.That(result.IsFailure).IsTrue();
            await Assert.That(result.Error).IsAssignableTo<ValidationError>();
            await Assert.That(((ValidationError)result.Error).Errors.Keys).Contains("Email");
        }

        await inner.DidNotReceive().Handle(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Validation_decorator_invokes_inner_when_valid()
    {
        var inner = Substitute.For<ICommandHandler<RegisterUserCommand>>();
        inner.Handle(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success()));

        var provider = new ServiceCollection()
            .AddSingleton<IValidator<RegisterUserCommand>>(new RegisterUserValidator())
            .BuildServiceProvider();

        var decorator = new ValidationCommandDecorator<RegisterUserCommand>(provider, inner);

        var result = await decorator.Handle(new RegisterUserCommand(Email: "student@smartlms.local"), CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await inner.Received(1).Handle(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Transaction_decorator_commits_on_success_and_rolls_back_on_failure()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.HasActiveTransaction.Returns(false);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        unitOfWork.RollbackAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var successInner = Substitute.For<ICommandHandler<PingCommand>>();
        successInner.Handle(Arg.Any<PingCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success()));
        var successDecorator = new TransactionCommandDecorator<PingCommand>(successInner, unitOfWork);

        var failureInner = Substitute.For<ICommandHandler<PingCommand>>();
        failureInner.Handle(Arg.Any<PingCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Failure(Error.Conflict("X", "conflict"))));
        var failureDecorator = new TransactionCommandDecorator<PingCommand>(failureInner, unitOfWork);

        var successResult = await successDecorator.Handle(new PingCommand(), CancellationToken.None);
        var failureResult = await failureDecorator.Handle(new PingCommand(), CancellationToken.None);

        using (Assert.Multiple())
        {
            await Assert.That(successResult.IsSuccess).IsTrue();
            await Assert.That(failureResult.IsFailure).IsTrue();
        }

        _ = unitOfWork.Received(2).HasActiveTransaction;
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Transaction_decorator_reuses_ambient_transaction()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.HasActiveTransaction.Returns(true);

        var inner = Substitute.For<ICommandHandler<PingCommand>>();
        inner.Handle(Arg.Any<PingCommand>(), Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(Result.Success()));

        var decorator = new TransactionCommandDecorator<PingCommand>(inner, unitOfWork);

        await decorator.Handle(new PingCommand(), CancellationToken.None);

        await unitOfWork.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}

// ----- Test messages -----

public sealed record PingCommand : ICommand;

public sealed record EchoCommand(string Message) : ICommand<string>;

public sealed record GetGreetingQuery : IQuery<string>;

public sealed record RegisterUserCommand(string Email) : ICommand;

internal sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
