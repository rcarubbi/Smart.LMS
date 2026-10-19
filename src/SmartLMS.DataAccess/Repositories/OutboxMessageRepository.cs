using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.DataAccess.Repositories;

/// <summary>
/// Reads outbox messages through the ambient DbContext so processed/failed
/// mutations on returned entities are tracked by the caller's SaveChanges.
/// </summary>
internal sealed class OutboxMessageRepository(ISmartLmsDbContext context) : IOutboxMessageRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(
        int batchSize,
        int maxAttempts,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (context is not DbContext db)
        {
            throw new InvalidOperationException("Outbox claiming requires the EF Core DbContext implementation.");
        }

        var claimId = Guid.NewGuid();
        const string sql = """
            ;WITH batch AS (
                SELECT TOP (@batchSize) *
                FROM dbo.OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE ProcessedOn IS NULL
                  AND Attempts < @maxAttempts
                  AND (NextAttemptAtUtc IS NULL OR NextAttemptAtUtc <= @nowUtc)
                  AND (LeaseUntilUtc IS NULL OR LeaseUntilUtc <= @nowUtc)
                ORDER BY CreatedAt
            )
            UPDATE batch
            SET ClaimId = @claimId, LeaseUntilUtc = @leaseUntilUtc;
            """;

        await db.Database.ExecuteSqlRawAsync(sql,
        [
            new SqlParameter("@batchSize", batchSize),
            new SqlParameter("@maxAttempts", maxAttempts),
            new SqlParameter("@nowUtc", nowUtc),
            new SqlParameter("@leaseUntilUtc", nowUtc.Add(leaseDuration)),
            new SqlParameter("@claimId", claimId),
        ], cancellationToken);

        return await context.OutboxMessages
            .Where(m => m.ClaimId == claimId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken = default)
        => await context.OutboxMessages.CountAsync(m => m.ProcessedOn == null && m.Attempts < maxAttempts, cancellationToken);
}
