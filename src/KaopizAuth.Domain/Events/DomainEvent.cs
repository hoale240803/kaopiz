namespace KaopizAuth.Domain.Events;

/// <summary>
/// Base interface for domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the unique identifier for the event
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// Base implementation for domain events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;

    /// <inheritdoc />
    public Guid EventId { get; private set; } = Guid.NewGuid();
}
