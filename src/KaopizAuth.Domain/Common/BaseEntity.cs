namespace KaopizAuth.Domain.Common;

/// <summary>
/// Interface for entities that support audit trail
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// When this entity was created
    /// </summary>
    DateTimeOffset CreatedTime { get; set; }

    /// <summary>
    /// When this entity was last updated
    /// </summary>
    DateTimeOffset LastUpdatedTime { get; set; }

    /// <summary>
    /// Who created this entity
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Who last updated this entity
    /// </summary>
    string? LastUpdatedBy { get; set; }
}

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// When this entity was soft deleted
    /// </summary>
    DateTimeOffset? DeletedTime { get; set; }

    /// <summary>
    /// Who soft deleted this entity
    /// </summary>
    string? DeletedBy { get; set; }
}

/// <summary>
/// Base entity class with common properties including soft delete and audit trail
/// </summary>
/// <typeparam name="TKey">The type of the primary key</typeparam>
public abstract class BaseEntity<TKey> : IAuditableEntity, ISoftDeletableEntity 
    where TKey : IEquatable<TKey>
{
    protected BaseEntity()
    {
        CreatedTime = LastUpdatedTime = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets or sets the unique identifier for this entity
    /// </summary>
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets when this entity was created
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets when this entity was last updated
    /// </summary>
    public DateTimeOffset LastUpdatedTime { get; set; }

    /// <summary>
    /// Gets or sets who created this entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets who last updated this entity
    /// </summary>
    public string? LastUpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets when this entity was soft deleted
    /// </summary>
    public DateTimeOffset? DeletedTime { get; set; }

    /// <summary>
    /// Gets or sets who soft deleted this entity
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Indicates whether this entity has been soft deleted
    /// </summary>
    public bool IsDeleted => DeletedTime.HasValue;

    /// <summary>
    /// Gets or sets whether this entity is active (not soft deleted)
    /// </summary>
    public bool IsActive 
    { 
        get => !IsDeleted;
        set 
        {
            if (!value && !IsDeleted)
            {
                DeletedTime = DateTimeOffset.UtcNow;
            }
            else if (value && IsDeleted)
            {
                DeletedTime = null;
                DeletedBy = null;
            }
        }
    }

    /// <summary>
    /// Soft delete this entity
    /// </summary>
    /// <param name="deletedBy">Who is deleting this entity</param>
    public virtual void SoftDelete(string? deletedBy = null)
    {
        DeletedTime = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
        LastUpdatedTime = DateTimeOffset.UtcNow;
        LastUpdatedBy = deletedBy;
    }

    /// <summary>
    /// Restore this entity from soft delete
    /// </summary>
    /// <param name="restoredBy">Who is restoring this entity</param>
    public virtual void Restore(string? restoredBy = null)
    {
        DeletedTime = null;
        DeletedBy = null;
        LastUpdatedTime = DateTimeOffset.UtcNow;
        LastUpdatedBy = restoredBy;
    }

    /// <summary>
    /// Mark this entity as updated
    /// </summary>
    /// <param name="updatedBy">Who is updating this entity</param>
    public virtual void MarkAsUpdated(string? updatedBy = null)
    {
        LastUpdatedTime = DateTimeOffset.UtcNow;
        LastUpdatedBy = updatedBy;
    }
}

/// <summary>
/// Base entity with Guid as primary key
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
