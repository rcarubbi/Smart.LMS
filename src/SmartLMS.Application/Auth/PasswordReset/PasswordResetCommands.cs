using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.UserAccess;
using ErrorOr;

namespace SmartLMS.Application.Auth.PasswordReset;

public sealed record RequestPasswordResetInput(string Email);
public sealed record RequestPasswordResetCommand(RequestPasswordResetInput Input) : ICommand, IOwnsTransaction;

internal sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetValidator() =>
        RuleFor(c => c.Input.Email).NotEmpty().EmailAddress().MaximumLength(256);
}

internal sealed class RequestPasswordResetHandler(
    ISmartLmsDbContext context,
    IEmailSender emailSender,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<RequestPasswordResetHandler> logger) : ICommandHandler<RequestPasswordResetCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            u => u.Email == command.Input.Email && u.Active, cancellationToken);
        if (user is null)
        {
            return Result.Success;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var old in await context.PasswordResetTokens
                     .Where(t => t.UserId == user.Id && t.UsedAtUtc == null)
                     .ToListAsync(cancellationToken))
        {
            old.MarkUsed(now);
        }

        var token = CreateToken();
        context.PasswordResetTokens.Add(new PasswordResetToken(
            user.Id,
            Hash(token),
            now,
            now.AddMinutes(30)));
        await context.SaveChangesAsync(cancellationToken);

        var baseUrl = configuration["App:BaseUrl"] ?? await context.Parameters.AsNoTracking()
            .Where(p => p.Key == Parameter.BaseUrlKey)
            .Select(p => p.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "http://localhost:3000";
        var link = $"{baseUrl.TrimEnd('/')}/reset-password?token={token}";
        try
        {
            await emailSender.SendAsync(
                user.Email,
                "Reset your SmartLMS password",
                $"<p>Use this single-use link within 30 minutes:</p><p><a href=\"{HtmlEncoder.Default.Encode(link)}\">Reset password</a></p>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Keep account-enumeration behavior identical; another request can issue a fresh link.
            logger.LogWarning(ex, "Could not send a password reset message to {Email}", user.Email);
        }

        return Result.Success;
    }

    private static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    internal static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public sealed record ResetPasswordInput(string Token, string NewPassword);
public sealed record ResetPasswordCommand(ResetPasswordInput Input) : ICommand;

internal sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(c => c.Input.Token).NotEmpty();
        RuleFor(c => c.Input.NewPassword).NotEmpty().MinimumLength(8);
    }
}

internal sealed class ResetPasswordHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : ICommandHandler<ResetPasswordCommand>
{
    public async ValueTask<ErrorOr<Success>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = RequestPasswordResetHandler.Hash(command.Input.Token);
        var token = await context.PasswordResetTokens.Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (token is null || !token.IsUsable(now) || !token.User.Active)
        {
            return Error.Unauthorized("Auth.InvalidResetToken", "The password reset link is invalid or expired.");
        }

        // Conditional update makes token consumption single-use under concurrent requests.
        var consumed = await context.PasswordResetTokens
            .Where(t => t.Id == token.Id && t.UsedAtUtc == null && t.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.UsedAtUtc, now), cancellationToken);
        if (consumed != 1)
        {
            return Error.Unauthorized("Auth.InvalidResetToken", "The password reset link is invalid or expired.");
        }

        token.User.UpdatePasswordHash(passwordHasher.Hash(command.Input.NewPassword));
        foreach (var refresh in await context.RefreshTokens
                     .Where(t => t.UserId == token.UserId && t.RevokedAtUtc == null)
                     .ToListAsync(cancellationToken))
        {
            refresh.Revoke(now);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
}
