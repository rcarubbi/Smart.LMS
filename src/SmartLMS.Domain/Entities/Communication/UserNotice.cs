using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class UserNotice
{
    public UserNotice(Guid userId, Guid noticeId, DateTime visualizationDateTime)
    {
        UserId = userId;
        NoticeId = noticeId;
        VisualizationDateTime = visualizationDateTime;
    }

    // ORM materialization.
    private UserNotice()
    {
    }

    public Guid UserId { get; private set; }

    private User _user = null!;

    public User User
    {
        get => _user;
        private set => _user = value;
    }

    public Guid NoticeId { get; private set; }

    private Notice _notice = null!;

    public Notice Notice
    {
        get => _notice;
        private set => _notice = value;
    }

    private DateTime _visualizationDateTime;

    public DateTime VisualizationDateTime
    {
        get => _visualizationDateTime;
        private set => _visualizationDateTime = value;
    }
}
