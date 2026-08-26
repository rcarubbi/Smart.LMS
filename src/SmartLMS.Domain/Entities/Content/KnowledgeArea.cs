namespace SmartLMS.Domain.Entities.Content;

public class KnowledgeArea : Entity
{
    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Subject> Subjects { get; set; } = [];
}
