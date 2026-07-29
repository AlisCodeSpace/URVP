using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.Auth
{
    /// <summary>
    /// Handles the UserLoggedInEvent by updating login timestamps and checking for suspicious activity
    /// </summary>
    public class UserLoggedInEventHandler : IEventHandler<UserLoggedInEvent>
    {
        private readonly ILogger<UserLoggedInEventHandler> _logger;
        private readonly AppDbContext _context;
        private readonly INotificationRepository _notificationRepository;
        
        public UserLoggedInEventHandler(
            ILogger<UserLoggedInEventHandler> logger,
            AppDbContext context,
            INotificationRepository notificationRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }
        
        public async Task HandleAsync(UserLoggedInEvent domainEvent)
        {
            try
            {
                _logger.LogInformation(
                    "Handling UserLoggedInEvent for user {UserId} with provider {Provider} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.AuthenticationProvider, domainEvent.CorrelationId);
                
                // 1. Update last login timestamp
                await UpdateLastLoginTimestampAsync(domainEvent.UserId, domainEvent.LoginTimestamp);
                
                // 2. Check for suspicious login patterns
                var isSuspicious = await CheckForSuspiciousActivityAsync(domainEvent);
                
                // 3. Send security notification if needed
                if (isSuspicious)
                {
                    await SendSecurityNotificationAsync(domainEvent);
                }
                
                _logger.LogInformation(
                    "Successfully processed UserLoggedInEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling UserLoggedInEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
        }
        
        private async Task UpdateLastLoginTimestampAsync(Guid userId, DateTime loginTimestamp)
        {
            try
            {
                // Find the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found when updating last login timestamp", userId);
                    return;
                }
                
                // In a future enhancement, we could add a LastLoginAt field to the User entity
                // For now, we'll just log the login
                _logger.LogInformation("User {UserId} logged in at {LoginTimestamp}", userId, loginTimestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update last login timestamp for user {UserId}", userId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
        
        private Task<bool> CheckForSuspiciousActivityAsync(UserLoggedInEvent domainEvent)
        {
            // This is a placeholder for future security enhancements
            // In a real implementation, we would check for:
            // - Logins from unusual locations
            // - Multiple failed login attempts before success
            // - Logins at unusual times
            // - Multiple logins in a short period from different locations
            
            // For now, we'll just return false (no suspicious activity)
            return Task.FromResult(false);
        }
        
        private async Task SendSecurityNotificationAsync(UserLoggedInEvent domainEvent)
        {
            try
            {
                // Create a security notification for the user
                var notification = new Notification
                {
                    UserId = domainEvent.UserId,
                    Title = "Unusual Login Activity Detected",
                    Message = $"We detected a login to your account from a new location or device at {domainEvent.LoginTimestamp:g}. " +
                              "If this was you, you can ignore this message. Otherwise, please contact support immediately.",
                    Type = "SecurityAlert",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Priority = NotificationPriority.High.ToString().ToLower()
                };
                
                await _notificationRepository.CreateAsync(notification);
                
                _logger.LogInformation(
                    "Sent security notification to user {UserId} about suspicious login",
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security notification to user {UserId}", domainEvent.UserId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
    }
}

