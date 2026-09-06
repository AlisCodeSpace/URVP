using FEA.URVP.Application.DTOs.Notifications;
using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetSettings;

public sealed record GetNotificationSettingsQuery(Guid UserId)
    : IRequest<UserNotificationSettingsDto?>;
