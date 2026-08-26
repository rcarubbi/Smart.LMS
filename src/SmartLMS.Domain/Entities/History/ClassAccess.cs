using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.History;

public class ClassAccess
{
    public long Id { get; private set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Content.Class Class { get; set; } = null!;

    public DateTime AccessDateTime { get; set; }

    /// <summary>Percentage of the class watched (0-100).</summary>
    public int Percentual { get; set; }

    public decimal WatchedSeconds { get; set; }
}
