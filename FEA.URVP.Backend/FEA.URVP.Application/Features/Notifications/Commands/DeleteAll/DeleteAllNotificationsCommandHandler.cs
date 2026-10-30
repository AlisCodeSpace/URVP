using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Common;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Features.Notifications.Commands.DeleteAll;

public sealed class DeleteAllNotificationsCommandHandler : BaseCommandHandler<DeleteAllNotificationsCommand, int>
{
    private readonly INotificationRepository _notifications;
    private readonly IEventBus _eventBus;

    public DeleteAllNotificationsCommandHandler(
        ILogger<DeleteAllNotificationsCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INotificationRepository notifications,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _notifications = notifications;
        _eventBus = eventBus;
    }

    protected override async Task<int> HandleInternal(
        DeleteAllNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        var count = await UnitOfWork.ExecuteInTransactionAsync(
            ct => _notifications.DeleteAllAsync(request.UserId, ct),
            cancellationToken);

        if (count > 0)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new NotificationDeletedEvent(Guid.Empty, request.UserId),
                Logger,
                cancellationToken);
        }

        return count;
    }
}
