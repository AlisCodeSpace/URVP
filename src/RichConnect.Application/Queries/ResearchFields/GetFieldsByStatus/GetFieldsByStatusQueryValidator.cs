using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldsByStatus
{
    public class GetFieldsByStatusQueryValidator : AbstractValidator<GetFieldsByStatusQuery>
    {
        public GetFieldsByStatusQueryValidator()
        {
            // Status is an enum, so we don't need to validate it explicitly
            // as the compiler ensures it's a valid value
            
            // Optional pagination parameters
            When(q => q.PageNumber.HasValue, () => {
                RuleFor(q => q.PageNumber!.Value)
                    .GreaterThan(0)
                    .WithMessage("Page number must be greater than 0.");
            });
            
            When(q => q.PageSize.HasValue, () => {
                RuleFor(q => q.PageSize!.Value)
                    .GreaterThan(0)
                    .WithMessage("Page size must be greater than 0.")
                    .LessThanOrEqualTo(100)
                    .WithMessage("Page size cannot exceed 100 items per page.");
            });
        }
    }
}

