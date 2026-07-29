using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Partners.RejectPartner
{
    /// <summary>
    /// Validator for RejectPartnerCommand
    /// </summary>
    public class RejectPartnerCommandValidator : AbstractValidator<RejectPartnerCommand>
    {
        private readonly IPartnerRepository _partnerRepository;

        public RejectPartnerCommandValidator(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;

            RuleFor(x => x.PartnerId)
                .NotEmpty().WithMessage("Partner ID is required.");

            RuleFor(x => x.AdminUserId)
                .NotEmpty().WithMessage("Admin user ID is required.");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters long.")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.");

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
                .WithMessage("Only pending partners can be rejected.");
        }
    }
}