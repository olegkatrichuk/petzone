namespace PetZone.SharedKernel;

public abstract class SoftDeletableEntity<TId> : Entity<TId>, ISoftDeletable
    where TId : IComparable<TId>
{
    public bool IsDeleted { get; private set; }
    public DateTime? DeletionDate { get; private set; }

    protected SoftDeletableEntity(TId id) : base(id) { }
    protected SoftDeletableEntity() { }

    public virtual void Delete()
    {
        IsDeleted = true;
        DeletionDate = DateTime.UtcNow;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletionDate = null;
    }
}