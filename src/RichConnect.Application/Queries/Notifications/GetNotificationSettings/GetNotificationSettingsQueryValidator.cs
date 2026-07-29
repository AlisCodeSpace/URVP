using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationSettings;

public class GetNotificationSettingsQueryValidator : AbstractValidator<GetNotificationSettingsQuery>
{
    public GetNotificationSettingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

