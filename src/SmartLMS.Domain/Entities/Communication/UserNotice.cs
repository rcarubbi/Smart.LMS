using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class UserNotice
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public long NoticeId { get; set; }

    public Notice Notice { get; set; } = null!;

    public DateTime VisualizationDateTime { get; set; }
}
