using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Handlers.Auth
{
    /// <summary>
    /// Handles the UserAuthenticatedEvent by checking for pending notifications and refreshing user data
    /// </summary>
    public class UserAuthenticatedEventHandler : IEventHandler<UserAuthenticatedEvent>
    {
        private readonly ILogger<UserAuthenticatedEventHandler> _logger;
        private readonly AppDbContext _context;
        private readonly INotificationRepository _notificationRepository;
        
        public UserAuthenticatedEventHandler(
            ILogger<UserAuthenticatedEventHandler> logger,
            AppDbContext context,
            INotificationRepository notificationRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }
        
        public async Task HandleAsync(UserAuthenticatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation(
                    "Handling UserAuthenticatedEvent for user {UserId} with provider {Provider} and method {Method} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.AuthenticationProvider, domainEvent.AuthenticationMethod, domainEvent.CorrelationId);
                
                // 1. Check for pending notifications
                await CheckForPendingNotificationsAsync(domainEvent.UserId);
                
                // 2. Update user's last active timestamp (could be added to User entity in the future)
                await UpdateUserActivityAsync(domainEvent.UserId);
                
                // 3. Log successful authentication
                _logger.LogInformation(
                    "Successfully processed UserAuthenticatedEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling UserAuthenticatedEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
        }
        
        private async Task CheckForPendingNotificationsAsync(Guid userId)
        {
            try
            {
                // Get unread notification count
                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
                
                if (unreadCount > 0)
                {
                    _logger.LogInformation("User {UserId} has {UnreadCount} unread notifications", userId, unreadCount);
                    
                    // In a real implementation, this might trigger a real-time notification
                    // through SignalR or similar technology to notify the user's client
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check pending notifications for user {UserId}", userId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
        
        private async Task UpdateUserActivityAsync(Guid userId)
        {
            try
            {
                // Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found when updating activity timestamp", userId);
                    return;
                }
                
                // In a future enhancement, we could add a LastActiveAt field to the User entity
                // For now, we'll just update the UpdatedAt field
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated activity timestamp for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update activity timestamp for user {UserId}", userId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
    }
}

