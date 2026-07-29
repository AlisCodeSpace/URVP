using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.RejectChallenge
{
    public class RejectChallengeCommandValidator : AbstractValidator<RejectChallengeCommand>
    {
        private readonly IChallengeRepository _repository;

        public RejectChallengeCommandValidator(IChallengeRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("Challenge does not exist");

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required");

            RuleFor(x => x.RejectDto.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters");

            // Validate that challenge is in Pending status before rejection
            RuleFor(x => x.ChallengeId)
                .MustAsync(async (id, cancellation) =>
                {
                    var challenge = await _repository.GetByIdAsync(id);
                    return challenge?.Status == ChallengeStatus.Pending;
                })
                .WithMessage("Only pending challenges can be rejected");
        }
    }
}
