using FEA.URVP.Domain.Events;

namespace FEA.URVP.Application.Abstractions.Events;

/// <summary>
/// Publishes domain events to registered handlers.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;

    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
