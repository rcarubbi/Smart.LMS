using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Users.DeleteUser;

/// <summary>Soft-deletes a teacher or student (moves it to the trash).</summary>
public sealed record DeleteUserCommand(Guid Id) : ICommand;

internal sealed class DeleteUserHandler(ISmartLmsDbContext context, TimeProvider timeProvider)
    : ICommandHandler<DeleteUserCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([command.Id], cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found.");
        }

        if (user.Kind == UserRole.Admin)
        {
            return Error.Conflict("Users.CannotDeleteAdmin", "Não é possível excluir um administrador.");
        }

        user.Deactivate();
        var activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == command.Id && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
