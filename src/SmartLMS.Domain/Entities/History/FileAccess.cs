using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.History;

public class FileAccess
{
    public FileAccess(Guid userId, Guid fileId, DateTime accessDateTime)
    {
        UserId = userId;
        FileId = fileId;
        AccessDateTime = accessDateTime;
    }

    // ORM materialization.
    private FileAccess()
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

    public Guid FileId { get; private set; }

    private SupportFile _file = null!;

    public SupportFile File
    {
        get => _file;
        private set => _file = value;
    }

    private DateTime _accessDateTime;

    public DateTime AccessDateTime
    {
        get => _accessDateTime;
        private set => _accessDateTime = value;
    }
}
