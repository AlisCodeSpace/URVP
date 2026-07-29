namespace RICHConnect.Backend.Application.Interfaces.Notifications
{
    /// <summary>
    /// Service for caching notification counts and related data
    /// </summary>
    public interface INotificationCacheService
    {
        /// <summary>
        /// Get cached unread count for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Unread count, or null if not cached</returns>
        Task<int?> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// Update cached unread count for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="count">The unread count</param>
        /// <returns>True if successfully updated</returns>
        Task<bool> UpdateUnreadCountAsync(Guid userId, int count);

        /// <summary>
        /// Invalidate (clear) cached unread count for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if successfully invalidated</returns>
        Task<bool> InvalidateUnreadCountAsync(Guid userId);

        /// <summary>
        /// Increment cached unread count for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>New count</returns>
        Task<int> IncrementUnreadCountAsync(Guid userId);

        /// <summary>
        /// Decrement cached unread count for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>New count</returns>
        Task<int> DecrementUnreadCountAsync(Guid userId);
    }
}
