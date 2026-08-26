using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Domain.Entities.Content;

public class SupportFile : Entity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Relative path inside the file storage volume.</summary>
    public string StoragePath { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<FileAccess> FileAccesses { get; set; } = [];
}
