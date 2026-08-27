namespace SmartLMS.Domain.Entities.History;

/// <summary>Audit trail entry written by the persistence layer on every entity change.</summary>
public class AuditLog
{
    public AuditLog(Guid entityId, string type, string oldState, string newState, DateTime dateTime, Guid? userId)
    {
        EntityId = entityId;
        Type = type;
        OldState = oldState;
        NewState = newState;
        DateTime = dateTime;
        UserId = userId;
    }

    // ORM materialization.
    private AuditLog()
    {
    }

    public long Id { get; private set; }

    private string _oldState = string.Empty;

    public string OldState
    {
        get => _oldState;
        private set => _oldState = value ?? string.Empty;
    }

    private string _newState = string.Empty;

    public string NewState
    {
        get => _newState;
        private set => _newState = value ?? string.Empty;
    }

    private DateTime _dateTime;

    public DateTime DateTime
    {
        get => _dateTime;
        private set => _dateTime = value;
    }

    /// <summary>Id of the acting user, if any (no FK so audits never block deletes).</summary>
    public Guid? UserId { get; private set; }

    public Guid EntityId { get; private set; }

    private string _type = string.Empty;

    /// <summary>Short type name of the audited entity.</summary>
    public string Type
    {
        get => _type;
        private set => _type = value ?? string.Empty;
    }
}
