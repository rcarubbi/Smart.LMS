namespace SmartLMS.Domain.Entities;

public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; protected set; }

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }
}
