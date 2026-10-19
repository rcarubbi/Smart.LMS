using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.Application.Abstractions;

/// <summary>
/// Persistence contract for reading outbox messages for async processing.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>Atomically claims an eligible batch; expired leases allow crash recovery.</summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        int maxAttempts,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <param name="maxAttempts">Must match GetUnprocessedAsync so counts agree on poison cut-off.</param>
    Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken = default);
}
