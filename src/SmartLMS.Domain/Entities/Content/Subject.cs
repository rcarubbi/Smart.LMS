using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Content;

public class Subject : Entity
{
    public Subject(string name, int order, Guid knowledgeAreaId)
    {
        Name = name;
        Order = order;
        KnowledgeAreaId = knowledgeAreaId;
    }

    // ORM materialization.
    private Subject()
    {
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        private set => _name = new PersonName(value).Value;
    }

    private int _order;

    public int Order
    {
        get => _order;
        private set => _order = new NonNegativeInteger(value).Value;
    }

    public Guid KnowledgeAreaId { get; private set; }

    public KnowledgeArea KnowledgeArea { get; private set; } = null!;

    public ICollection<Course> Courses { get; private set; } = [];

    public void Rename(string name) => Name = name;

    public void SetOrder(int order) => Order = new NonNegativeInteger(order).Value;

    public void MoveTo(Guid knowledgeAreaId) => KnowledgeAreaId = knowledgeAreaId;
}
