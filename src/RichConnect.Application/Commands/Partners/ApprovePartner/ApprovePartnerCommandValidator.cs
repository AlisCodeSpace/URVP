using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Partners.ApprovePartner
{
    /// <summary>
    /// Validator for ApprovePartnerCommand
    /// </summary>
    public class ApprovePartnerCommandValidator : AbstractValidator<ApprovePartnerCommand>
    {
        private readonly IPartnerRepository _partnerRepository;

        public ApprovePartnerCommandValidator(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;

            RuleFor(x => x.PartnerId)
                .NotEmpty().WithMessage("Partner ID is required.");

            RuleFor(x => x.AdminUserId)
                .NotEmpty().WithMessage("Admin user ID is required.");

            // Check if partner exists
            RuleFor(x => x.PartnerId)
                .MustAsync(async (partnerId, cancellation) => 
                {
                    var partner = await _partnerRepository.GetByIdAsync(partnerId);
                    return partner != null;
                })
                .WithMessage("Partner not found.");

            // Check if partner is in pending status
            RuleFor(x => x.PartnerId)
                .MustAsync(async (partnerId, cancellation) => 
                {
                    var partner = await _partnerRepository.GetByIdAsync(partnerId);
                    return partner != null && partner.Status == ApprovalStatus.Pending;
                })
                .WithMessage("Only pending partners can be approved.");
        }
    }
}