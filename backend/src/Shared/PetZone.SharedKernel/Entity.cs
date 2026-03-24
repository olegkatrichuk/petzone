namespace PetZone.SharedKernel;

public abstract class Entity<TId> where TId : IComparable<TId>
{
    public TId Id { get; private set; }

    protected Entity(TId id) => Id = id;

    protected Entity() { }
}