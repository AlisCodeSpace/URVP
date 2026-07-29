namespace FEA.URVP.Domain.Events;

/// <summary>
/// Marker interface for domain events raised by aggregates.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}
