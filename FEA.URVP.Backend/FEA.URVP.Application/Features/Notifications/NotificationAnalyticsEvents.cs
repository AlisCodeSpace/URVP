namespace FEA.URVP.Application.Notifications;

public static class NotificationAnalyticsEvents
{
    public const string Intended = "notification_intended";
    public const string EmailQueued = "email_queued";
    public const string EmailQueueFailed = "email_queue_failed";
    public const string EmailSkipped = "email_skipped";
    public const string EmailSent = "email_sent";
    public const string EmailSendFailed = "email_send_failed";
    public const string Created = "notification_created";
    public const string Read = "notification_read";
}
