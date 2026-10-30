using FEA.URVP.Application.Features.Notifications.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetUserNotifications;

public sealed record GetUserNotificationsQuery(
    Guid UserId,
    int PageNumber,
    int PageSize,
    bool? IsRead = null) : IRequest<(IReadOnlyList<NotificationDto> Items, int TotalCount)>;
