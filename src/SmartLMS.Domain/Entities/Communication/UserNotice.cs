using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class UserNotice
{
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public Guid NoticeId { get; set; }

    public virtual Notice Notice { get; set; } = null!;

    public DateTime VisualizationDateTime { get; set; }
}
