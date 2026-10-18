using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Users.UpdateUser;

public sealed record UpdateUserInput(string Name, string Email, string? NewPassword = null);
public sealed record UpdateUserCommand(Guid Id, UpdateUserInput Input) : ICommand;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Input.NewPassword).MinimumLength(8).When(c => !string.IsNullOrWhiteSpace(c.Input.NewPassword));
    }
}

internal sealed class UpdateUserHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : ICommandHandler<UpdateUserCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.Id == command.Id, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found.");
        }

        var duplicate = await context.Users.AnyAsync(
            u => u.Id != command.Id && u.Email == command.Input.Email,
            cancellationToken);
        if (duplicate)
        {
            return Error.Conflict("Users.EmailAlreadyExists", "Este e-mail já está cadastrado.");
        }

        user.Rename(command.Input.Name);
        user.ChangeEmail(command.Input.Email);
        if (!string.IsNullOrWhiteSpace(command.Input.NewPassword))
        {
            user.UpdatePasswordHash(passwordHasher.Hash(command.Input.NewPassword));
            var now = timeProvider.GetUtcNow().UtcDateTime;
            foreach (var token in await context.RefreshTokens
                         .Where(t => t.UserId == user.Id && t.RevokedAtUtc == null)
                         .ToListAsync(cancellationToken))
            {
                token.Revoke(now);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
