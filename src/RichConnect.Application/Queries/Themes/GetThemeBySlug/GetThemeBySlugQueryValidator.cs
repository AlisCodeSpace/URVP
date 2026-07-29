using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeBySlug
{
    public class GetThemeBySlugQueryValidator : AbstractValidator<GetThemeBySlugQuery>
    {
        public GetThemeBySlugQueryValidator()
        {
            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Theme slug is required.")
                .MaximumLength(128).WithMessage("Theme slug cannot exceed 128 characters.")
                .Matches(@"^[a-z0-9-]+$").WithMessage("Theme slug can only contain lowercase letters, numbers, and hyphens.");
        }
    }
}

