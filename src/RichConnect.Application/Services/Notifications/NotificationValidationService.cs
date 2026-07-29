using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class NotificationValidationService
    {
        private readonly NotificationBusinessRulesService _businessRulesService;
        private readonly ILogger<NotificationValidationService> _logger;
        
        public NotificationValidationService(
            NotificationBusinessRulesService businessRulesService,
            ILogger<NotificationValidationService> logger)
        {
            _businessRulesService = businessRulesService;
            _logger = logger;
        }
        
        public async Task<ValidationResult> ValidateNotificationCreationAsync(
            Guid userId, 
            NotificationType type, 
            string notificationChannel = "push")
        {
            try
            {
                _logger.LogDebug("Validating notification creation for user {UserId}, type {Type}, channel {Channel}", 
                    userId, type, notificationChannel);
                
                var canReceive = await _businessRulesService.ValidateUserCanReceiveNotification(
                    userId, notificationChannel);
                var withinLimit = await _businessRulesService.ValidateNotificationLimit(userId);
                
                if (!canReceive)
                {
                    _logger.LogWarning("User {UserId} has disabled {Channel} notifications", userId, notificationChannel);
                    return ValidationResult.Failure($"User has disabled {notificationChannel} notifications");
                }
                
                if (!withinLimit)
                {
                    _logger.LogWarning("User {UserId} has exceeded notification limit (advisory only)", userId);
                    // Don't return failure - just log warning
                }
                
                _logger.LogDebug("Notification creation validation passed for user {UserId}", userId);
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating notification creation for user {UserId}", userId);
                return ValidationResult.Failure("Validation error occurred");
            }
        }
        
        public async Task<ValidationResult> ValidateNotificationAccessAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var hasAccess = await _businessRulesService.ValidateNotificationAccess(notificationId, userId);
                
                if (!hasAccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to notification {NotificationId}", 
                        userId, notificationId);
                    return ValidationResult.Failure("User does not have access to this notification");
                }
                
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating notification access for user {UserId}, notification {NotificationId}", 
                    userId, notificationId);
                return ValidationResult.Failure("Access validation error occurred");
            }
        }
        
        public async Task<bool> ShouldSendEmailNotificationAsync(Guid userId, NotificationType type)
        {
            try
            {
                return await _businessRulesService.ShouldSendEmailNotification(userId, type.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email notification eligibility for user {UserId}", userId);
                return false;
            }
        }
        
        public async Task<bool> ShouldSendPushNotificationAsync(Guid userId, NotificationType type)
        {
            try
            {
                return await _businessRulesService.ShouldSendPushNotification(userId, type.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking push notification eligibility for user {UserId}", userId);
                return false;
            }
        }
    }
    
    public class ValidationResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        
        private ValidationResult(bool isSuccess, string errorMessage = "")
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        
        public static ValidationResult Success() => new(true);
        public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
    }
}
