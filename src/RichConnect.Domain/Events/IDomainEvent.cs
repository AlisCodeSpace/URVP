namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Base interface for all domain events
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        Guid EventId { get; }
        
        /// <summary>
        /// When this event occurred
        /// </summary>
        DateTime OccurredAt { get; }
        
        /// <summary>
        /// Type of the event for categorization
        /// </summary>
        string EventType { get; }
    }
}
