using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for Challenge rejection requests
    /// </summary>
    public class RejectChallengeDtoValidator : AbstractValidator<RejectChallengeDto>
    {
        public RejectChallengeDtoValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(ChallengeValidationConstants.REJECTION_REASON_MAX_LENGTH)
                .WithMessage($"Rejection reason cannot exceed {ChallengeValidationConstants.REJECTION_REASON_MAX_LENGTH} characters")
                .MinimumLength(ChallengeValidationConstants.REJECTION_REASON_MIN_LENGTH)
                .WithMessage($"Rejection reason must be at least {ChallengeValidationConstants.REJECTION_REASON_MIN_LENGTH} characters long");
        }
    }
} 