namespace FEA.URVP.Application.Common.Options;

public sealed class NotificationCacheOptions
{
    public const string SectionName = "NotificationCache";

    public int ExpirationMinutes { get; set; } = 5;
}
