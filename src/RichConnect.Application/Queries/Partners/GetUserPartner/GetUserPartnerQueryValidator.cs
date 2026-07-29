using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Partners.GetUserPartner
{
    /// <summary>
    /// Validator for GetUserPartnerQuery
    /// </summary>
    public class GetUserPartnerQueryValidator : AbstractValidator<GetUserPartnerQuery>
    {
        public GetUserPartnerQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}