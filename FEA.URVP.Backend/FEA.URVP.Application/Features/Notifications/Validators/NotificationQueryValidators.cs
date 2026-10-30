using FEA.URVP.Application.Features.Notifications.Queries.GetById;
using FEA.URVP.Application.Features.Notifications.Queries.GetSettings;
using FEA.URVP.Application.Features.Notifications.Queries.GetUnreadCount;
using FEA.URVP.Application.Features.Notifications.Queries.GetUserNotifications;
using FluentValidation;

namespace FEA.URVP.Application.Features.Notifications.Validators;

public sealed class GetUserNotificationsQueryValidator : AbstractValidator<GetUserNotificationsQuery>
{
    public GetUserNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}

public sealed class GetUnreadCountQueryValidator : AbstractValidator<GetUnreadCountQuery>
{
    public GetUnreadCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("NotificationId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public sealed class GetNotificationSettingsQueryValidator
    : AbstractValidator<GetNotificationSettingsQuery>
{
    public GetNotificationSettingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
