using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;

namespace SmartLMS.Application.Auth.ChangePassword;

public sealed record ChangePasswordInput(string CurrentPassword, string NewPassword);

public sealed record ChangePasswordCommand(ChangePasswordInput Input) : ICommand;

internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(c => c.Input.CurrentPassword).NotEmpty();
        RuleFor(c => c.Input.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .NotEqual(c => c.Input.CurrentPassword)
            .WithMessage("The new password must be different from the current password.");
    }
}

internal sealed class ChangePasswordHandler(
    ISmartLmsDbContext context,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : ICommandHandler<ChangePasswordCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("Auth.NotAuthenticated", "No authenticated user.");
        }

        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || !user.Active || !passwordHasher.Verify(command.Input.CurrentPassword, user.PasswordHash))
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "The current password is incorrect.");
        }

        user.UpdatePasswordHash(passwordHasher.Hash(command.Input.NewPassword));

        // Revoke every active session: a password change invalidates existing refresh tokens.
        var activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
