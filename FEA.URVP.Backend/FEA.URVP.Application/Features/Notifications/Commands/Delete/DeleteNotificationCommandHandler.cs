using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.Delete;

public sealed class DeleteNotificationCommandHandler
    : BaseCommandHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationRepository _notifications;
    private readonly NotificationValidationService _validation;
    private readonly IEventBus _eventBus;

    public DeleteNotificationCommandHandler(
        ILogger<DeleteNotificationCommandHandler> logger,
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
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        if (deleted)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new NotificationDeletedEvent(request.NotificationId, request.UserId),
                Logger,
                cancellationToken);
        }

        return deleted;
    }

    private async Task<bool> PersistAsync(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null)
        {
            return false;
        }

        _validation.ValidateNotificationAccess(notification, request.UserId);
        return await _notifications.DeleteAsync(request.NotificationId, request.UserId, cancellationToken);
    }
}
