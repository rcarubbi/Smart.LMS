using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

internal sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}

internal sealed class LogoutHandler(
    ISmartLmsDbContext context,
    ITokenService tokenService,
    TimeProvider timeProvider) : ICommandHandler<LogoutCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(command.RefreshToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var stored = await context.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAtUtc == null, cancellationToken);

        if (stored is not null)
        {
            stored.Revoke(now);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Idempotent: logging out with an already-revoked/unknown token still succeeds.
        return Result.Success;
    }
}
