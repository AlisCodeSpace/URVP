using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Interface for handling domain events
    /// </summary>
    public interface IEventHandler<T> where T : IDomainEvent
    {
        /// <summary>
        /// Handles a domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to handle</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task HandleAsync(T domainEvent);
    }
}
