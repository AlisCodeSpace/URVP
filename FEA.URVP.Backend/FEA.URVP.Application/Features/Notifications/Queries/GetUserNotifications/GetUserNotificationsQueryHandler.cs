using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Notifications.DTOs;
using FEA.URVP.Application.Features.Notifications.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetUserNotifications;

public sealed class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, (IReadOnlyList<NotificationDto> Items, int TotalCount)>
{
    private readonly INotificationRepository _notifications;

    public GetUserNotificationsQueryHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<(IReadOnlyList<NotificationDto> Items, int TotalCount)> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _notifications.GetUserNotificationsAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.IsRead,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
