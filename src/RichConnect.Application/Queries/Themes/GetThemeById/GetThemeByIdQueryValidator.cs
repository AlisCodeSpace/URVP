using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeById
{
    public class GetThemeByIdQueryValidator : AbstractValidator<GetThemeByIdQuery>
    {
        public GetThemeByIdQueryValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty().WithMessage("Theme ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid theme ID is required.");
        }
    }
}
