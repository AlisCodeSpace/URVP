using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldBySlug
{
    public class GetFieldBySlugQueryValidator : AbstractValidator<GetFieldBySlugQuery>
    {
        public GetFieldBySlugQueryValidator()
        {
            RuleFor(query => query.Slug)
                .NotEmpty()
                .WithMessage("A valid research field slug is required.")
                .MaximumLength(200)
                .WithMessage("Slug cannot exceed 200 characters.");
        }
    }
}

