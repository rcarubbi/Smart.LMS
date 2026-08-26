using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Domain.Entities.Content;

public class SupportFile : Entity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Relative path inside the file storage volume.</summary>
    public string StoragePath { get; set; } = string.Empty;

    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Class Class { get; set; } = null!;

    public ICollection<FileAccess> FileAccesses { get; set; } = [];
}
