using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.UpdateNotificationSettings;

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        // Business rule: At least one notification type must be enabled
        RuleFor(x => x)
            .Must(HaveAtLeastOneNotificationTypeEnabled)
            .WithMessage("At least one notification type must be enabled (Email or In-App)");
    }

    private static bool HaveAtLeastOneNotificationTypeEnabled(UpdateNotificationSettingsCommand command)
    {
        return command.EmailNotifications || command.InAppNotifications;
    }
}

