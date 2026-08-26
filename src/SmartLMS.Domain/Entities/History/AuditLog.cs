namespace SmartLMS.Domain.Entities.History;

/// <summary>Audit trail entry written by the persistence layer on every entity change.</summary>
public class AuditLog
{
    public long Id { get; private set; }

    public string OldState { get; set; } = string.Empty;

    public string NewState { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    /// <summary>Id of the acting user, if any (no FK so audits never block deletes).</summary>
    public Guid? UserId { get; set; }

    public Guid EntityId { get; set; }

    /// <summary>Short type name of the audited entity.</summary>
    public string Type { get; set; } = string.Empty;
}
