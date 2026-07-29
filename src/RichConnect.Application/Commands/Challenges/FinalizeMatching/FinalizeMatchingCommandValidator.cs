using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.Validators.Challenges;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.FinalizeMatching
{
    /// <summary>
    /// Validator for FinalizeMatchingCommand
    /// </summary>
    public class FinalizeMatchingCommandValidator : AbstractValidator<FinalizeMatchingCommand>
    {
        private readonly IChallengeRepository _repository;
        private readonly ChallengeBusinessRulesValidator _businessRulesValidator;

        public FinalizeMatchingCommandValidator(IChallengeRepository repository, ChallengeBusinessRulesValidator businessRulesValidator)
        {
            _repository = repository;
            _businessRulesValidator = businessRulesValidator;

            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("Challenge does not exist");

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required");

            RuleFor(x => x.ChallengeId)
                .MustAsync(async (challengeId, cancellation) => 
                {
                    var challenge = await _repository.GetByIdAsync(challengeId);
                    return challenge?.Status == ChallengeStatus.Approved || challenge?.Status == ChallengeStatus.Matched;
                })
                .WithMessage("Only approved or already matched challenges can be finalized");

            RuleFor(x => x.ChallengeId)
                .MustAsync(async (challengeId, cancellation) => 
                {
                    var challenge = await _repository.GetByIdAsync(challengeId);
                    return challenge?.MatchingStatus == ChallengeMatchingStatus.AwaitingApproval;
                })
                .WithMessage("All invited professors must respond before finalizing the match");

            RuleFor(x => x.ChallengeId)
                .MustAsync(async (challengeId, cancellation) => 
                {
                    var invites = await _repository.GetInvitesByChallengeAsync(challengeId);
                    var acceptedInvites = invites.Where(i => i.Status == InviteStatus.Accepted).ToList();
                    return acceptedInvites.Count > 0;
                })
                .WithMessage("Cannot finalize matching because no professors accepted the invite");

            // Business Rules Validation
            RuleFor(x => x)
                .MustAsync(async (command, cancellation) => 
                {
                    var matchingResult = await _businessRulesValidator.ValidateMatchingOperationsAsync(command.ChallengeId);
                    return matchingResult.IsValid;
                })
                .WithMessage("Matching operations validation failed");

            RuleFor(x => x)
                .MustAsync(async (command, cancellation) => 
                {
                    var finalizationResult = await _businessRulesValidator.ValidateFinalizationRulesAsync(command.ChallengeId);
                    return finalizationResult.IsValid;
                })
                .WithMessage("Finalization rules validation failed");
        }
    }
}
