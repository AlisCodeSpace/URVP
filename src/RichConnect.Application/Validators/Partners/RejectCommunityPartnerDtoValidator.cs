using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Partners;

namespace RICHConnect.Backend.Application.Validators.Partners
{
    /// <summary>
    /// Validator for CommunityPartner rejection requests
    /// </summary>
    public class RejectCommunityPartnerDtoValidator : AbstractValidator<RejectCommunityPartnerDto>
    {
        public RejectCommunityPartnerDtoValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.")
                .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters long.");
        }
    }
} 