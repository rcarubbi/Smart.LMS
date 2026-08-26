using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth.Models;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;

internal sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty();
    }
}

internal sealed class LoginHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer) : ICommandHandler<LoginCommand, AuthResponse>
{
    public async ValueTask<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null || !user.Active || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        return Result.Success(await tokenIssuer.IssueAsync(user, cancellationToken));
    }
}
