using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.ValueObjects;
using ClassAccess = SmartLMS.Domain.Entities.History.ClassAccess;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Domain.Entities.UserAccess;

public abstract class User : Entity
{
    protected User(string name, string email)
    {
        Name = name;
        Email = email;
    }

    // Parameterless constructor for ORM materialization only.
    protected User()
    {
    }

    private string _name = string.Empty;

    /// <summary>Display name of the user.</summary>
    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    private string _email = string.Empty;

    /// <summary>Unique e-mail used to authenticate and contact the user.</summary>
    public string Email
    {
        get => _email;
        private set => _email = new EmailAddress(value).Value;
    }

    /// <summary>BCrypt hash. Never store plaintext.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public ICollection<UserNotice> VisitedNotices { get; private set; } = [];

    public ICollection<ClassAccess> ClassAccesses { get; private set; } = [];

    public ICollection<FileAccess> FileAccesses { get; private set; } = [];

    public ICollection<Notice> PrivateNotices { get; private set; } = [];

    /// <summary>Updates the display name (validated by <see cref="PersonName"/>).</summary>
    public void Rename(string name) => Name = name;

    /// <summary>Sets a newly computed password hash (already hashed; never stores plaintext).</summary>
    public void UpdatePasswordHash(string hash) => PasswordHash = hash;

    /// <summary>Updates the e-mail address (validated by <see cref="EmailAddress"/>).</summary>
    public void ChangeEmail(string email) => Email = email;
}
