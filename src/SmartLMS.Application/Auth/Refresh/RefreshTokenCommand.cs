using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth.Models;
using SmartLMS.Application.Common;

namespace SmartLMS.Application.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}

/// <summary>Rotates the refresh token on every use (the presented token is revoked).</summary>
internal sealed class RefreshTokenHandler(
    ISmartLmsDbContext context,
    ITokenService tokenService,
    AuthTokenIssuer tokenIssuer,
    TimeProvider timeProvider) : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async ValueTask<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = tokenService.HashRefreshToken(command.RefreshToken);

        var stored = await context.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (stored is null || !stored.IsActive(now))
        {
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid or expired."));
        }

        stored.Revoke(now);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(await tokenIssuer.IssueAsync(stored.User, cancellationToken));
    }
}
