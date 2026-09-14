using SmartLMS.Domain.ValueObjects;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Domain.Entities.Content;

public class SupportFile : Entity
{
    public SupportFile(string name, string storagePath, Guid classId)
    {
        Name = name;
        StoragePath = storagePath;
        ClassId = classId;
    }

    // ORM materialization.
    private SupportFile()
    {
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    private string _storagePath = string.Empty;

    /// <summary>Relative path inside the file storage volume.</summary>
    public string StoragePath
    {
        get => _storagePath;
        private set => _storagePath = ArgumentNotEmpty(value, nameof(value)).Trim();
    }

    public Guid ClassId { get; private set; }

    public virtual Class Class { get; private set; } = null!;

    public virtual ICollection<FileAccess> FileAccesses { get; private set; } = [];

    public void Rename(string name) => Name = name;

    private static string ArgumentNotEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value!;
    }
}
