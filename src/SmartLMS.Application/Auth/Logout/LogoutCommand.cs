using Ardalis.Specification.EntityFrameworkCore;
using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Auth.Logout;

public sealed record LogoutInput(string RefreshToken);

public sealed record LogoutCommand(LogoutInput Input) : ICommand;

internal sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(c => c.Input.RefreshToken).NotEmpty();
    }
}

internal sealed class LogoutHandler(
    ISmartLmsDbContext context,
    ITokenService tokenService,
    TimeProvider timeProvider) : ICommandHandler<LogoutCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(command.Input.RefreshToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Tracking on purpose: Revoke mutates + SaveChanges; null stays idempotent success.
        var stored = await SpecificationEvaluator.Default
            .GetQuery(context.RefreshTokens, new ActiveRefreshTokenByHashSpec(tokenHash))
            .SingleOrDefaultAsync(cancellationToken);

        if (stored is not null)
        {
            stored.Revoke(now);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Idempotent: logging out with an already-revoked/unknown token still succeeds.
        return Result.Success;
    }
}
