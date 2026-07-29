using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.ApproveChallenge
{
    public class ApproveChallengeCommandValidator : AbstractValidator<ApproveChallengeCommand>
    {
        private readonly IChallengeRepository _repository;

        public ApproveChallengeCommandValidator(IChallengeRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("Challenge does not exist");

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required");

            // Validate that challenge is in Pending status before approval
            RuleFor(x => x.ChallengeId)
                .MustAsync(async (id, cancellation) =>
                {
                    var challenge = await _repository.GetByIdAsync(id);
                    return challenge?.Status == ChallengeStatus.Pending;
                })
                .WithMessage("Only pending challenges can be approved");
        }
    }
}
