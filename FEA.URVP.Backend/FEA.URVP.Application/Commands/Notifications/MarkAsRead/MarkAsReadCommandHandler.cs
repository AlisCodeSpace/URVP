using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.MarkAsRead;

public sealed class MarkAsReadCommandHandler : BaseCommandHandler<MarkAsReadCommand, bool>
{
    private readonly INotificationRepository _notifications;
    private readonly NotificationValidationService _validation;
    private readonly IEventBus _eventBus;

    public MarkAsReadCommandHandler(
        ILogger<MarkAsReadCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INotificationRepository notifications,
        NotificationValidationService validation,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _notifications = notifications;
        _validation = validation;
        _eventBus = eventBus;
    }

    protected override async Task<bool> HandleInternal(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var outcome = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        if (outcome is { Success: true, Publish: true, ReadAt: { } readAt })
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new NotificationReadEvent(request.NotificationId, request.UserId, readAt),
                Logger,
                cancellationToken);
        }

        return outcome.Success;
    }

    private async Task<MarkOutcome> PersistAsync(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
        {
            return new MarkOutcome(false, false, null);
        }

        _validation.ValidateNotificationAccess(notification, request.UserId);

        if (notification.IsRead)
        {
            return new MarkOutcome(true, false, notification.ReadAt);
        }

        var marked = await _notifications.MarkAsReadAsync(
            request.NotificationId,
            request.UserId,
            cancellationToken);

        if (!marked)
        {
            return new MarkOutcome(false, false, null);
        }

        var readAt = DateTime.UtcNow;
        notification.IsRead = true;
        notification.ReadAt = readAt;
        return new MarkOutcome(true, true, readAt);
    }

    private sealed record MarkOutcome(bool Success, bool Publish, DateTime? ReadAt);
}
