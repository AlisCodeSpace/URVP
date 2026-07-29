using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Notifications;

namespace RICHConnect.Backend.Application.Validators.Notifications
{
    public class UserNotificationSettingsDtoValidator : AbstractValidator<UserNotificationSettingsDto>
    {
        public UserNotificationSettingsDtoValidator()
        {
            // All boolean properties are required and must be valid boolean values
            // The DTO properties are already booleans, so we don't need additional validation
            // but we can add custom business rules if needed
            
            // Example: If email notifications are disabled, in-app notifications should still be enabled
            RuleFor(x => x)
                .Must(HaveAtLeastOneNotificationTypeEnabled)
                .WithMessage("At least one notification type must be enabled (Email or In-App)");
        }

        private static bool HaveAtLeastOneNotificationTypeEnabled(UserNotificationSettingsDto settings)
        {
            return settings.EmailNotifications || settings.InAppNotifications;
        }
    }
}
