using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;

namespace RICHConnect.Backend.Application.Queries.Partners.GetPartnerById
{
    /// <summary>
    /// Validator for GetPartnerByIdQuery
    /// </summary>
    public class GetPartnerByIdQueryValidator : AbstractValidator<GetPartnerByIdQuery>
    {
        private readonly IPartnerRepository _partnerRepository;

        public GetPartnerByIdQueryValidator(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;

            RuleFor(x => x.PartnerId)
                .NotEmpty().WithMessage("Partner ID is required.");

            // Check if partner exists
            RuleFor(x => x.PartnerId)
                .MustAsync(async (partnerId, cancellation) => 
                {
                    var partner = await _partnerRepository.GetByIdAsync(partnerId);
                    return partner != null;
                })
                .WithMessage("Partner not found.");
        }
    }
}