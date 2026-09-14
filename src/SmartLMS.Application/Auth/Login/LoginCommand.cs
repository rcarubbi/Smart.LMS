using Ardalis.Specification.EntityFrameworkCore;
using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth.Models;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Auth.Login;

public sealed record LoginInput(string Email, string Password);

public sealed record LoginCommand(LoginInput Input) : ICommand<AuthResponse>;

internal sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Input.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Input.Password).NotEmpty();
    }
}

internal sealed class LoginHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer) : ICommandHandler<LoginCommand, AuthResponse>
{
    public async ValueTask<ErrorOr<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var spec = new UserByEmailSpec(command.Input.Email);
        var user = await SpecificationEvaluator.Default
            .GetQuery(context.Users, spec)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null || !user.Active || !passwordHasher.Verify(command.Input.Password, user.PasswordHash))
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
        }

        return await tokenIssuer.IssueAsync(user, cancellationToken);
    }
}
