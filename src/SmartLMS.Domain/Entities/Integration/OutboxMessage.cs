namespace SmartLMS.Domain.Entities.Integration;

/// <summary>
/// An integration message persisted in the same transaction as the aggregate
/// change that produced it, then drained asynchronously by the outbox worker.
/// </summary>
public class OutboxMessage
{
    public const int MaxAttempts = 5;

    public OutboxMessage(string type, string payload, Guid? correlationId = null)
    {
        Type = type;
        Payload = payload;
        CorrelationId = correlationId;
        CreatedAt = DateTime.UtcNow;
    }

    // ORM materialization.
    private OutboxMessage()
    {
    }

    public long Id { get; private set; }

    private string _type = string.Empty;

    public string Type
    {
        get => _type;
        private set => _type = value ?? string.Empty;
    }

    private string _payload = string.Empty;

    /// <summary>Serialized event payload (JSON).</summary>
    public string Payload
    {
        get => _payload;
        private set => _payload = value ?? string.Empty;
    }

    public Guid? CorrelationId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ProcessedOn { get; private set; }

    public string? Error { get; private set; }

    public int Attempts { get; private set; }

    public Guid? ClaimId { get; private set; }

    public DateTime? LeaseUntilUtc { get; private set; }

    public DateTime? NextAttemptAtUtc { get; private set; }

    public void MarkProcessed(DateTime processedOn)
    {
        ProcessedOn = processedOn;
        ClaimId = null;
        LeaseUntilUtc = null;
        NextAttemptAtUtc = null;
        Error = null;
    }

    public void MarkFailed(string? error) => MarkFailed(error, DateTime.UtcNow);

    public void MarkFailed(string? error, DateTime failedAtUtc)
    {
        Error = error;
        Attempts++;
        ClaimId = null;
        LeaseUntilUtc = null;
        var retryMinutes = Math.Min(60, 1 << Math.Min(Attempts - 1, 6));
        NextAttemptAtUtc = Attempts >= MaxAttempts ? null : failedAtUtc.AddMinutes(retryMinutes);
        ProcessedOn = null;
    }

    public void Retry()
    {
        Attempts = 0;
        Error = null;
        ClaimId = null;
        LeaseUntilUtc = null;
        NextAttemptAtUtc = null;
        ProcessedOn = null;
    }
}
