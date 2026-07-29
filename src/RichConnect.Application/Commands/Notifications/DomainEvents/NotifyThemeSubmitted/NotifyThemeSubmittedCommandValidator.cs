using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmitted
{
    public class NotifyThemeSubmittedCommandValidator : AbstractValidator<NotifyThemeSubmittedCommand>
    {
        public NotifyThemeSubmittedCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty()
                .WithMessage("Theme ID is required");

            RuleFor(x => x.SubmittedByUserId)
                .NotEmpty()
                .WithMessage("Submitted by user ID is required");
        }
    }
}
