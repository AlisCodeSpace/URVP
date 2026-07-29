using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Themes.RejectTheme
{
    public class RejectThemeCommandValidator : AbstractValidator<RejectThemeCommand>
    {
        public RejectThemeCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty().WithMessage("Theme ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid theme ID is required.");

            RuleFor(x => x.RejectedBy)
                .NotEmpty().WithMessage("Rejector ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid rejector ID is required.");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.");
        }
    }
}
