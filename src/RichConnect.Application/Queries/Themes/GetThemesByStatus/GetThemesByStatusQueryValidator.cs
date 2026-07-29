using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemesByStatus
{
    public class GetThemesByStatusQueryValidator : AbstractValidator<GetThemesByStatusQuery>
    {
        public GetThemesByStatusQueryValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("A valid approval status is required.");

            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).When(x => x.UserId.HasValue)
                .WithMessage("A valid user ID is required when specified.");
        }
    }
}
