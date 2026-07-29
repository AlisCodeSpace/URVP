using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAsRead;

public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("NotificationId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

