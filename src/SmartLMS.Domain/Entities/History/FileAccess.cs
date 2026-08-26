using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.History;

public class FileAccess
{
    public long Id { get; private set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid FileId { get; set; }

    public SupportFile File { get; set; } = null!;

    public DateTime AccessDateTime { get; set; }
}
