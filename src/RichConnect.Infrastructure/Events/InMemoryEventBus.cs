using RICHConnect.Backend.Application.Handlers;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Infrastructure.Events
{
    /// <summary>
    /// In-memory implementation of the event bus
    /// </summary>
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryEventBus> _logger;

        public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T domainEvent) where T : IDomainEvent
        {
            _logger.LogInformation("Publishing domain event: {EventType} with ID: {EventId}", 
                domainEvent.EventType, domainEvent.EventId);

            try
            {
                // Get all handlers for this event type
                var handlers = _serviceProvider.GetServices<IEventHandler<T>>();
                
                if (!handlers.Any())
                {
                    _logger.LogWarning("No handlers found for event type: {EventType}", domainEvent.EventType);
                    return;
                }

                _logger.LogInformation("Found {HandlerCount} handlers for event type: {EventType}", 
                    handlers.Count(), domainEvent.EventType);

                // Execute all handlers in parallel
                var tasks = handlers.Select(handler => 
                    ExecuteHandler(handler, domainEvent));
                
                await Task.WhenAll(tasks);

                _logger.LogInformation("Successfully published domain event: {EventType} with ID: {EventId}", 
                    domainEvent.EventType, domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing domain event: {EventType} with ID: {EventId}", 
                    domainEvent.EventType, domainEvent.EventId);
                throw;
            }
        }

        public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            var tasks = domainEvents.Select(PublishAsync);
            await Task.WhenAll(tasks);
        }

        private async Task ExecuteHandler<T>(IEventHandler<T> handler, T domainEvent) where T : IDomainEvent
        {
            try
            {
                _logger.LogDebug("Executing handler {HandlerType} for event {EventType} with ID: {EventId}", 
                    handler.GetType().Name, domainEvent.EventType, domainEvent.EventId);

                await handler.HandleAsync(domainEvent);

                _logger.LogDebug("Successfully executed handler {HandlerType} for event {EventType} with ID: {EventId}", 
                    handler.GetType().Name, domainEvent.EventType, domainEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing handler {HandlerType} for event {EventType} with ID: {EventId}", 
                    handler.GetType().Name, domainEvent.EventType, domainEvent.EventId);
                // Don't rethrow - other handlers should still execute
            }
        }
    }
}
