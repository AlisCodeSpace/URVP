using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notifications;
    private readonly INotificationCacheService _cache;

    public GetUnreadCountQueryHandler(
        INotificationRepository notifications,
        INotificationCacheService cache)
    {
        _notifications = notifications;
        _cache = cache;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetUnreadCountAsync(request.UserId, cancellationToken);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var count = await _notifications.GetUnreadCountAsync(request.UserId, cancellationToken);
        await _cache.UpdateUnreadCountAsync(request.UserId, count, cancellationToken);
        return count;
    }
}
