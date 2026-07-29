using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Infrastructure.Events
{
    /// <summary>
    /// Interface for publishing domain events
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes a domain event to all registered handlers
        /// </summary>
        /// <typeparam name="T">The type of domain event</typeparam>
        /// <param name="domainEvent">The domain event to publish</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PublishAsync<T>(T domainEvent) where T : IDomainEvent;
        
        /// <summary>
        /// Publishes multiple domain events
        /// </summary>
        /// <param name="domainEvents">The domain events to publish</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PublishAsync(IEnumerable<IDomainEvent> domainEvents);
    }
}
