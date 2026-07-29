using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Themes.GetAllThemes
{
    public class GetAllThemesQueryValidator : AbstractValidator<GetAllThemesQuery>
    {
        public GetAllThemesQueryValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("A valid approval status is required when specified.");

            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).When(x => x.UserId.HasValue)
                .WithMessage("A valid user ID is required when specified.");

            RuleFor(x => x.ResearchFieldId)
                .NotEqual(Guid.Empty).When(x => x.ResearchFieldId.HasValue)
                .WithMessage("A valid research field ID is required when specified.");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.SearchTerm))
                .WithMessage("Search term cannot exceed 100 characters.");

            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate).When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("To date must be greater than or equal to from date.");
        }
    }
}
