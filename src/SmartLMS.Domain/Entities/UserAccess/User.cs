using SmartLMS.Domain.Entities.Communication;
using ClassAccess = SmartLMS.Domain.Entities.History.ClassAccess;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Domain.Entities.UserAccess;

public abstract class User : Entity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hash. Never store plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<UserNotice> VisitedNotices { get; set; } = [];

    public ICollection<ClassAccess> ClassAccesses { get; set; } = [];

    public ICollection<FileAccess> FileAccesses { get; set; } = [];

    public ICollection<Notice> PrivateNotices { get; set; } = [];
}
