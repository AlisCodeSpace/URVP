using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeApproved
{
    public class NotifyThemeApprovedCommandValidator : AbstractValidator<NotifyThemeApprovedCommand>
    {
        public NotifyThemeApprovedCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty()
                .WithMessage("Theme ID is required");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty()
                .WithMessage("Approved by user ID is required");
        }
    }
}
