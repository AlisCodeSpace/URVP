namespace FEA.URVP.Application.DTOs.Notifications;

public sealed class UserNotificationSettingsDto
{
    public Guid UserId { get; init; }
    public bool EmailNotifications { get; init; }
    public bool InAppNotifications { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
