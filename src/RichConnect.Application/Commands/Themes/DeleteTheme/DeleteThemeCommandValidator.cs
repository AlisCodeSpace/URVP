using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Themes.DeleteTheme
{
    public class DeleteThemeCommandValidator : AbstractValidator<DeleteThemeCommand>
    {
        public DeleteThemeCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty().WithMessage("Theme ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid theme ID is required.");

            RuleFor(x => x.DeletedBy)
                .NotEmpty().WithMessage("Deleted by is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid deleter ID is required.");
        }
    }
}
