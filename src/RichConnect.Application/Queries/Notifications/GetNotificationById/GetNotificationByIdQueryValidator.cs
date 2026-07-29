using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationById;

public class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("NotificationId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

