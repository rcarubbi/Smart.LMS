using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class Comment : Entity
{
    public string CommentText { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Class Class { get; set; } = null!;
}
