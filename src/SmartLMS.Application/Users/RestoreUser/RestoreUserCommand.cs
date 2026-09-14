using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;

namespace SmartLMS.Application.Users.RestoreUser;

public sealed record RestoreUserCommand(Guid Id) : ICommand;

internal sealed class RestoreUserHandler(ISmartLmsDbContext context) : ICommandHandler<RestoreUserCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RestoreUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([command.Id], cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Users.NotFound", "User not found.");
        }

        user.Activate();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}