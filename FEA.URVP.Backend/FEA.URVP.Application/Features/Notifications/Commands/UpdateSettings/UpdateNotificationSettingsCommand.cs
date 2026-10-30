using FEA.URVP.Application.Features.Notifications.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.UpdateSettings;

public sealed record UpdateNotificationSettingsCommand(
    Guid UserId,
    bool EmailNotifications,
    bool InAppNotifications) : IRequest<UserNotificationSettingsDto>;
