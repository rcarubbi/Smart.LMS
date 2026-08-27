namespace SmartLMS.Domain.Entities;

/// <summary>
/// Base class for all domain entities. Provides identity and lifecycle state.
/// Identity (<see cref="Id"/>) and creation timestamp are immutable; lifecycle
/// (<see cref="Active"/>) is changed only through explicit domain behavior.
/// </summary>
public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Active = true;
    }

    public Guid Id { get; init; }

    public bool Active { get; private set; }

    public DateTime CreatedAt { get; init; }

    public void Activate() => Active = true;

    public void Deactivate() => Active = false;
}
