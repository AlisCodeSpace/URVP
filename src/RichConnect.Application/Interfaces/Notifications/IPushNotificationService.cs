namespace RICHConnect.Backend.Application.Interfaces.Notifications
{
    /// <summary>
    /// Service for sending push notifications to users
    /// </summary>
    public interface IPushNotificationService
    {
        /// <summary>
        /// Queue a push notification for delivery
        /// </summary>
        /// <param name="notificationId">The notification ID to send as push</param>
        /// <returns>True if successfully queued, false otherwise</returns>
        Task<bool> QueuePushNotificationAsync(Guid notificationId);

        /// <summary>
        /// Send a push notification immediately (for high priority notifications)
        /// </summary>
        /// <param name="userId">The user to send the notification to</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="data">Additional data payload</param>
        /// <returns>True if successfully sent, false otherwise</returns>
        Task<bool> SendPushNotificationAsync(Guid userId, string title, string message, Dictionary<string, string>? data = null);

        /// <summary>
        /// Register a device token for push notifications
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceToken">The device token (FCM, APNS, etc.)</param>
        /// <param name="platform">The platform (ios, android, web)</param>
        /// <returns>True if successfully registered</returns>
        Task<bool> RegisterDeviceTokenAsync(Guid userId, string deviceToken, string platform);

        /// <summary>
        /// Unregister a device token
        /// </summary>
        /// <param name="deviceToken">The device token to unregister</param>
        /// <returns>True if successfully unregistered</returns>
        Task<bool> UnregisterDeviceTokenAsync(string deviceToken);
    }
}
