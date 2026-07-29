using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class NotificationBusinessRulesService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationBusinessRulesService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _maxUnreadNotifications;

        public NotificationBusinessRulesService(
            INotificationRepository notificationRepository,
            IConfiguration configuration,
            ILogger<NotificationBusinessRulesService> logger)
        {
            _notificationRepository = notificationRepository;
            _configuration = configuration;
            _logger = logger;
            
            // Get max notifications from configuration or use default
            _maxUnreadNotifications = _configuration.GetValue<int>("NotificationSettings:MaxUnreadNotifications", 100);
        }

        public async Task<bool> ValidateUserCanReceiveNotification(Guid userId, string notificationType)
        {
            var settings = await _notificationRepository.GetUserSettingsAsync(userId);
            
            if (settings == null)
            {
                // Default to allowing notifications if no settings exist
                return true;
            }

            return notificationType switch
            {
                "email" => settings.EmailNotifications,
                "push" => settings.InAppNotifications,
                _ => true
            };
        }

        public async Task<bool> ValidateNotificationLimit(Guid userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);
            return unreadCount < _maxUnreadNotifications;
        }

        public async Task<bool> ValidateNotificationAccess(Guid notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            return notification?.UserId == userId;
        }

        public string DetermineNotificationPriority(string notificationType)
        {
            return notificationType switch
            {
                "challenge_submitted" => "high",
                "challenge_approved" => "high",
                "challenge_rejected" => "medium",
                "partner_registered" => "medium",
                "theme_approved" => "medium",
                "theme_rejected" => "low",
                _ => "low"
            };
        }

        public async Task<bool> ShouldSendEmailNotification(Guid userId, string notificationType)
        {
            var canReceive = await ValidateUserCanReceiveNotification(userId, "email");
            var withinLimit = await ValidateNotificationLimit(userId);
            
            return canReceive && withinLimit;
        }

        public async Task<bool> ShouldSendPushNotification(Guid userId, string notificationType)
        {
            var canReceive = await ValidateUserCanReceiveNotification(userId, "push");
            var withinLimit = await ValidateNotificationLimit(userId);
            
            return canReceive && withinLimit;
        }
    }
}

