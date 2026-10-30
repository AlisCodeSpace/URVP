namespace FEA.URVP.Application.Features.Notifications.DTOs;

public sealed class UpdateNotificationSettingsRequest
{
    public bool EmailNotifications { get; init; }

    public bool InAppNotifications { get; init; }
}
