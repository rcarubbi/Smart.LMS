using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.History;

namespace SmartLMS.DataAccess.Auditing;

/// <summary>
/// Writes an <see cref="AuditLog"/> row for every added/modified entity on SaveChanges,
/// replacing the XML audit mechanism of the legacy EF6 Context.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserProvider currentUserProvider) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            CollectAuditEntries(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            CollectAuditEntries(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void CollectAuditEntries(DbContext context)
    {
        var userId = currentUserProvider.GetCurrentUserId();
        var now = DateTime.UtcNow;

        // Materialize first: adding AuditLog entities below mutates the change tracker.
        var auditedEntries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is Entity
                        && e.State is EntityState.Added or EntityState.Modified
                        && e.Entity is not AuditLog)
            .ToList();

        if (auditedEntries.Count == 0)
        {
            return;
        }

        foreach (var entry in auditedEntries)
        {
            var entity = (Entity)entry.Entity;
            var newState = Serialize(entry, includeOriginalValues: false);

            string oldState = entry.State == EntityState.Modified
                ? Serialize(entry, includeOriginalValues: true)
                : string.Empty;

            context.Set<AuditLog>().Add(new AuditLog(
                entity.Id,
                entity.GetType().Name,
                oldState,
                newState,
                now,
                userId));
        }
    }

    private static string Serialize(EntityEntry entry, bool includeOriginalValues)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey() && !includeOriginalValues)
            {
                continue;
            }

            values[property.Metadata.Name] = includeOriginalValues && entry.State == EntityState.Modified
                ? property.OriginalValue
                : property.CurrentValue;
        }

        return JsonSerializer.Serialize(values, SerializerOptions);
    }
}
