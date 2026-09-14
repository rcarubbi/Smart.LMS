using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.DataAccess.Repositories;

/// <summary>
/// Reads outbox messages through the ambient DbContext so processed/failed
/// mutations on returned entities are tracked by the caller's SaveChanges.
/// </summary>
internal sealed class OutboxMessageRepository(ISmartLmsDbContext context) : IOutboxMessageRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default)
        => await context.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.Attempts < maxAttempts)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    public async Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken = default)
        => await context.OutboxMessages.CountAsync(m => m.ProcessedOn == null && m.Attempts < maxAttempts, cancellationToken);
}
