using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAllAsRead;

public class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

