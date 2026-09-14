using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.Application.Abstractions;

/// <summary>
/// Persistence contract for reading outbox messages for async processing.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <param name="maxAttempts">Messages at/above this attempt count are skipped (dead-letter).</param>
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default);

    /// <param name="maxAttempts">Must match GetUnprocessedAsync so counts agree on poison cut-off.</param>
    Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken = default);
}
