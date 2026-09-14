using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Users.DeleteUser;

/// <summary>Soft-deletes a teacher or student (moves it to the trash).</summary>
public sealed record DeleteUserCommand(Guid Id) : ICommand;

internal sealed class DeleteUserHandler(ISmartLmsDbContext context) : ICommandHandler<DeleteUserCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([command.Id], cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found.");
        }

        if (user is Admin)
        {
            return Error.Conflict("Users.CannotDeleteAdmin", "Não é possível excluir um administrador.");
        }

        user.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}