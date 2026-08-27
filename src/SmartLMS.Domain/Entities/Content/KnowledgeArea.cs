using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Content;

public class KnowledgeArea : Entity
{
    public KnowledgeArea(string name, int order)
    {
        Name = name;
        Order = order;
    }

    // ORM materialization.
    private KnowledgeArea()
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

    public ICollection<Subject> Subjects { get; private set; } = [];

    public void Rename(string name) => Name = name;

    public void SetOrder(int order) => Order = new NonNegativeInteger(order).Value;
}
