using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler : BaseCommandHandler<MarkAllAsReadCommand, int>
{
    private readonly INotificationRepository _notifications;
    private readonly INotificationCacheService _cache;

    public MarkAllAsReadCommandHandler(
        ILogger<MarkAllAsReadCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INotificationRepository notifications,
        INotificationCacheService cache)
        : base(logger, unitOfWork)
    {
        _notifications = notifications;
        _cache = cache;
    }

    protected override async Task<int> HandleInternal(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var count = await UnitOfWork.ExecuteInTransactionAsync(
            ct => _notifications.MarkAllAsReadAsync(request.UserId, ct),
            cancellationToken);

        if (count > 0)
        {
            await _cache.InvalidateUnreadCountAsync(request.UserId, cancellationToken);
        }

        return count;
    }
}
