namespace RICHConnect.Backend.Application.Interfaces.Analytics
{
    /// <summary>
    /// Service for tracking analytics events
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Track a notification event
        /// </summary>
        /// <param name="eventName">The event name (e.g., "notification_created", "notification_read")</param>
        /// <param name="userId">The user ID</param>
        /// <param name="properties">Additional event properties</param>
        /// <returns>True if successfully tracked</returns>
        Task<bool> TrackNotificationEventAsync(string eventName, Guid userId, Dictionary<string, object>? properties = null);

        /// <summary>
        /// Track a theme event
        /// </summary>
        /// <param name="eventName">The event name (e.g., "theme_submitted", "theme_approved")</param>
        /// <param name="themeId">The theme ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="properties">Additional event properties</param>
        /// <returns>True if successfully tracked</returns>
        Task<bool> TrackThemeEventAsync(string eventName, Guid themeId, Guid userId, Dictionary<string, object>? properties = null);

        /// <summary>
        /// Track a research field event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="researchFieldId">The research field ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="properties">Additional event properties</param>
        /// <returns>True if successfully tracked</returns>
        Task<bool> TrackResearchFieldEventAsync(string eventName, Guid researchFieldId, Guid userId, Dictionary<string, object>? properties = null);

        /// <summary>
        /// Track a challenge event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="challengeId">The challenge ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="properties">Additional event properties</param>
        /// <returns>True if successfully tracked</returns>
        Task<bool> TrackChallengeEventAsync(string eventName, Guid challengeId, Guid userId, Dictionary<string, object>? properties = null);

        /// <summary>
        /// Track a custom event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="properties">Event properties</param>
        /// <returns>True if successfully tracked</returns>
        Task<bool> TrackEventAsync(string eventName, Dictionary<string, object>? properties = null);
    }
}
