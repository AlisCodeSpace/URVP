namespace FEA.URVP.Application.Common.Options;

public sealed class NotificationSettingsOptions
{
    public const string SectionName = "NotificationSettings";

    public int MaxUnreadNotifications { get; set; } = 100;
}
