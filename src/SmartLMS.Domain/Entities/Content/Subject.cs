namespace SmartLMS.Domain.Entities.Content;

public class Subject : Entity
{
    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid KnowledgeAreaId { get; set; }

    public KnowledgeArea KnowledgeArea { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = [];
}
