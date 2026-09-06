namespace FEA.URVP.Application.Options;

public sealed class NotificationSettingsOptions
{
    public const string SectionName = "NotificationSettings";

    public int MaxUnreadNotifications { get; set; } = 100;
}
