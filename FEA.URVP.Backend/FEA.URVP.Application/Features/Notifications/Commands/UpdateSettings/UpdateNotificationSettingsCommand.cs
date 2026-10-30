using FEA.URVP.Application.DTOs.Notifications;
using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.UpdateSettings;

public sealed record UpdateNotificationSettingsCommand(
    Guid UserId,
    bool EmailNotifications,
    bool InAppNotifications) : IRequest<UserNotificationSettingsDto>;
