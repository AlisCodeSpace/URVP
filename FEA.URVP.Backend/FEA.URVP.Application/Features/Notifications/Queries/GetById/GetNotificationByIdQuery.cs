using FEA.URVP.Application.Features.Notifications.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetById;

public sealed record GetNotificationByIdQuery(Guid NotificationId, Guid UserId)
    : IRequest<NotificationDto>;
