using System.Collections.Concurrent;
using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Infrastructure.Events;

/// <summary>
/// In-process event bus that dispatches domain events to registered handlers.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent =>
        DispatchAsync(domainEvent, cancellationToken);

    public async Task PublishAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }

    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _logger.LogInformation(
            "Publishing domain event {EventType} ({EventId})",
            domainEvent.EventType,
            domainEvent.EventId);

        var eventType = domainEvent.GetType();
        var handlerInterface = HandlerTypeCache.GetOrAdd(
            eventType,
            static type => typeof(IEventHandler<>).MakeGenericType(type));

        var handlers = _serviceProvider.GetServices(handlerInterface).Cast<object>().ToList();
        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers registered for {EventType}", domainEvent.EventType);
            return;
        }

        var handleMethod = handlerInterface.GetMethod(nameof(IEventHandler<IDomainEvent>.HandleAsync))
            ?? throw new InvalidOperationException($"HandleAsync was not found on {handlerInterface.Name}.");

        foreach (var handler in handlers)
        {
            try
            {
                var task = (Task?)handleMethod.Invoke(handler, [domainEvent, cancellationToken])
                    ?? throw new InvalidOperationException("Event handler returned a null task.");

                await task;
            }
            catch (Exception ex)
            {
                // Isolate handler failures so remaining handlers still run.
                _logger.LogError(
                    ex,
                    "Handler {HandlerType} failed for event {EventType} ({EventId})",
                    handler.GetType().Name,
                    domainEvent.EventType,
                    domainEvent.EventId);
            }
        }
    }
}
