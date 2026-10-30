namespace FEA.URVP.Application.DTOs.Notifications;

public sealed class UpdateNotificationSettingsRequest
{
    public bool EmailNotifications { get; init; }

    public bool InAppNotifications { get; init; }
}
