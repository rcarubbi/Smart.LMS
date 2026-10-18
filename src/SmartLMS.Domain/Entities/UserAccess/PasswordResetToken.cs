namespace SmartLMS.Domain.Entities.UserAccess;

/// <summary>Single-use password-reset secret; only its SHA-256 hash is persisted.</summary>
public sealed class PasswordResetToken
{
    private PasswordResetToken() { }

    public PasswordResetToken(Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public long Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public bool IsUsable(DateTime nowUtc) => UsedAtUtc is null && ExpiresAtUtc > nowUtc;
    public void MarkUsed(DateTime nowUtc) => UsedAtUtc ??= nowUtc;
}
