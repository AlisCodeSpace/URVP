namespace FEA.URVP.Domain.Events;

/// <summary>
/// Convenience base type for domain events.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public virtual string EventType => GetType().Name;
}
