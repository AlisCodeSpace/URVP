using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Partners.GetPartnersByStatus
{
    /// <summary>
    /// Validator for GetPartnersByStatusQuery
    /// </summary>
    public class GetPartnersByStatusQueryValidator : AbstractValidator<GetPartnersByStatusQuery>
    {
        public GetPartnersByStatusQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

            RuleFor(x => x.SortBy)
                .Must(sortBy => string.IsNullOrEmpty(sortBy) || 
                    new[] { "SubmittedAt", "InstitutionName", "Status", "CreatedAt", "UpdatedAt" }.Contains(sortBy))
                .WithMessage("Invalid sort field. Valid values are: SubmittedAt, InstitutionName, Status, CreatedAt, UpdatedAt.");
        }
    }
}