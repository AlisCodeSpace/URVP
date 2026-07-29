using FEA.URVP.Domain.Events;

namespace FEA.URVP.Application.Abstractions.Events;

/// <summary>
/// Handles a specific domain event type.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
