using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Themes.GetUserThemes
{
    public class GetUserThemesQueryValidator : AbstractValidator<GetUserThemesQuery>
    {
        public GetUserThemesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid user ID is required.");

            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("A valid approval status is required when specified.");
        }
    }
}

