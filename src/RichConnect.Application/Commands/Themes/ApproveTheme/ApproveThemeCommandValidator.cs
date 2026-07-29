using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Themes.ApproveTheme
{
    public class ApproveThemeCommandValidator : AbstractValidator<ApproveThemeCommand>
    {
        public ApproveThemeCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty().WithMessage("Theme ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid theme ID is required.");

            RuleFor(x => x.ApprovedBy)
                .NotEmpty().WithMessage("Approver ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid approver ID is required.");
        }
    }
}
