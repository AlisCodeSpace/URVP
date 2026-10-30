using FEA.URVP.Application.Features.Notifications.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetSettings;

public sealed record GetNotificationSettingsQuery(Guid UserId)
    : IRequest<UserNotificationSettingsDto?>;
