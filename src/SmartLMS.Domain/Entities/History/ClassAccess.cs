using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.History;

public class ClassAccess
{
    public ClassAccess(Guid userId, Guid classId, int percentual, decimal watchedSeconds, DateTime accessDateTime)
    {
        UserId = userId;
        ClassId = classId;
        Percentual = percentual;
        WatchedSeconds = watchedSeconds;
        AccessDateTime = accessDateTime;
    }

    // ORM materialization.
    private ClassAccess()
    {
    }

    public long Id { get; private set; }

    public Guid UserId { get; private set; }

    private User _user = null!;

    public User User
    {
        get => _user;
        private set => _user = value;
    }

    public Guid ClassId { get; private set; }

    private Content.Class _class = null!;

    public Content.Class Class
    {
        get => _class;
        private set => _class = value;
    }

    private DateTime _accessDateTime;

    public DateTime AccessDateTime
    {
        get => _accessDateTime;
        private set => _accessDateTime = value;
    }

    private int _percentual;

    /// <summary>Percentage of the class watched (0-100).</summary>
    public int Percentual
    {
        get => _percentual;
        private set => _percentual = new Percentual(value).Value;
    }

    private decimal _watchedSeconds;

    public decimal WatchedSeconds
    {
        get => _watchedSeconds;
        private set => _watchedSeconds = value;
    }
}
