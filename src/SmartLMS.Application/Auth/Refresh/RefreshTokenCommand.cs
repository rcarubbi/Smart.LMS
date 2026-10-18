using Ardalis.Specification.EntityFrameworkCore;
using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Auth.Models;
using SmartLMS.Domain.Specifications;

namespace SmartLMS.Application.Auth.Refresh;

public sealed record RefreshTokenInput(string RefreshToken);

public sealed record RefreshTokenCommand(RefreshTokenInput Input) : ICommand<AuthResponse>;

internal sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(c => c.Input.RefreshToken).NotEmpty();
    }
}

/// <summary>Rotates the refresh token on every use (the presented token is revoked).</summary>
internal sealed class RefreshTokenHandler(
    ISmartLmsDbContext context,
    ITokenService tokenService,
    AuthTokenIssuer tokenIssuer,
    TimeProvider timeProvider) : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async ValueTask<ErrorOr<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = tokenService.HashRefreshToken(command.Input.RefreshToken);

        // Tracking on purpose: Revoke mutates + SaveChanges in the same unit of work.
        var stored = await SpecificationEvaluator.Default
            .GetQuery(context.RefreshTokens, new RefreshTokenByHashWithUserSpec(tokenHash))
            .SingleOrDefaultAsync(cancellationToken);

        if (stored is null || !stored.IsActive(now) || !stored.User.Active)
        {
            return Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid or expired.");
        }

        stored.Revoke(now);
        await context.SaveChangesAsync(cancellationToken);

        return await tokenIssuer.IssueAsync(stored.User, cancellationToken);
    }
}
