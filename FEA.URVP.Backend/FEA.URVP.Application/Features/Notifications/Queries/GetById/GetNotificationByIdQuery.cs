using FEA.URVP.Application.DTOs.Notifications;
using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetById;

public sealed record GetNotificationByIdQuery(Guid NotificationId, Guid UserId)
    : IRequest<NotificationDto>;
