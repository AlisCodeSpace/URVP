using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Domain.Entities.Notifications;
using MediatR;

namespace RICHConnect.Backend.Application.Handlers.Auth
{
    /// <summary>
    /// Handles the UserRegisteredEvent by creating notification settings and notifying admins
    /// </summary>
    public class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredEventHandler> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;
        
        public UserRegisteredEventHandler(
            ILogger<UserRegisteredEventHandler> logger,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            AppDbContext context,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        
        public async Task HandleAsync(UserRegisteredEvent domainEvent)
        {
            try
            {
                _logger.LogInformation(
                    "Handling UserRegisteredEvent for user {UserId} with provider {Provider} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.AuthenticationProvider, domainEvent.CorrelationId);
                
                // 1. Create default notification settings if they don't exist
                await CreateDefaultNotificationSettingsAsync(domainEvent.UserId);
                
                // 2. Notify admins about new user registration
                await NotifyAdminsAboutNewUserAsync(domainEvent);
                
                // 3. Log successful registration
                _logger.LogInformation(
                    "Successfully processed UserRegisteredEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error handling UserRegisteredEvent for user {UserId} [CorrelationId: {CorrelationId}]",
                    domainEvent.UserId, domainEvent.CorrelationId);
            }
        }
        
        private async Task CreateDefaultNotificationSettingsAsync(Guid userId)
        {
            try
            {
                // Check if notification settings already exist
                var existingSettings = await _notificationRepository.GetUserNotificationSettingsAsync(userId);
                if (existingSettings != null)
                {
                    _logger.LogInformation("Notification settings already exist for user {UserId}", userId);
                    return;
                }
                
                // Create default settings
                var settings = new UserNotificationSettings
                {
                    UserId = userId,
                    EmailNotifications = true,
                    InAppNotifications = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _notificationRepository.CreateUserNotificationSettingsAsync(settings);
                _logger.LogInformation("Created default notification settings for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default notification settings for user {UserId}", userId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
        
        private async Task NotifyAdminsAboutNewUserAsync(UserRegisteredEvent domainEvent)
        {
            try
            {
                // Get all admin users
                var adminUserIds = await _userRepository.GetAdminUserIdsAsync();
                if (adminUserIds.Count == 0)
                {
                    _logger.LogWarning("No admin users found to notify about new user registration");
                    return;
                }
                
                // Create notification for each admin
                foreach (var adminId in adminUserIds)
                {
                    // Create a notification using the notification command
                    var title = $"New {domainEvent.Role} Registration";
                    var message = $"{domainEvent.Name} ({domainEvent.Email}) has registered as a {domainEvent.Role} and their account is being set up.";
                    
                    // Create notification using the repository directly
                    var notification = new Notification
                    {
                        UserId = adminId,
                        Title = title,
                        Message = message,
                        Type = "UserRegistration",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        Data = domainEvent.UserId.ToString() // Store the new user's ID in the Data field
                    };
                    
                    await _notificationRepository.CreateAsync(notification);
                }
                
                _logger.LogInformation(
                    "Notified {AdminCount} admins about new user registration for {UserId}",
                    adminUserIds.Count, domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify admins about new user registration for {UserId}", domainEvent.UserId);
                // Don't rethrow - we don't want to fail the whole handler if just this part fails
            }
        }
    }
}

