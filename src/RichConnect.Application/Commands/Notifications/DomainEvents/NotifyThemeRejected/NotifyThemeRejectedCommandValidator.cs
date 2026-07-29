using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeRejected
{
    public class NotifyThemeRejectedCommandValidator : AbstractValidator<NotifyThemeRejectedCommand>
    {
        public NotifyThemeRejectedCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty()
                .WithMessage("Theme ID is required");

            RuleFor(x => x.RejectedByUserId)
                .NotEmpty()
                .WithMessage("Rejected by user ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required");
        }
    }
}
