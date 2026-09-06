using FEA.URVP.Application.DTOs.Notifications;
using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetUserNotifications;

public sealed record GetUserNotificationsQuery(
    Guid UserId,
    int PageNumber,
    int PageSize,
    bool? IsRead = null) : IRequest<(IReadOnlyList<NotificationDto> Items, int TotalCount)>;
