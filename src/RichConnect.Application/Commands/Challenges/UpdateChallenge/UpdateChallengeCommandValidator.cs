using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.Validators.Challenges;

namespace RICHConnect.Backend.Application.Commands.UpdateChallenge
{
    public class UpdateChallengeCommandValidator : AbstractValidator<UpdateChallengeCommand>
    {
        private readonly IChallengeRepository _repository;
        private readonly ChallengeBusinessRulesValidator _businessRulesValidator;

        public UpdateChallengeCommandValidator(IChallengeRepository repository, ChallengeBusinessRulesValidator businessRulesValidator)
        {
            _repository = repository;
            _businessRulesValidator = businessRulesValidator;

            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("Challenge does not exist");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required")
                .MustAsync(async (id, cancellation) => await _repository.ValidateResearchFieldExistsAsync(id))
                .WithMessage("The selected research field does not exist or is not approved");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative");

            RuleFor(x => x.UpdatedBy)
                .NotEmpty().WithMessage("Updated by is required");

            // Business Rules Validation - Note: Update restrictions are now handled at the service level
            // to allow admins to update any challenge while maintaining restrictions for partners

            RuleFor(x => x)
                .Must((command) => 
                {
                    var costResult = _businessRulesValidator.ValidateEstimatedCostConstraints(
                        command.EstimatedCost);
                    return costResult.IsValid;
                })
                .WithMessage("Estimated cost constraints validation failed")
                .When(x => x.EstimatedCost > 0);

            RuleFor(x => x)
                .MustAsync(async (command, cancellation) => 
                {
                    // Get the original challenge to use the actual owner for duplicate checking
                    var challenge = await _repository.GetByIdAsync(command.ChallengeId);
                    if (challenge == null) return false; // Will be caught by existence check above
                    
                    var duplicateResult = await _businessRulesValidator.ValidateDuplicatePreventionAsync(
                        command.Title, command.ResearchFieldId, challenge.SubmittedBy, command.ChallengeId);
                    return duplicateResult.IsValid;
                })
                .WithMessage("Duplicate challenge prevention validation failed");
        }
    }
}
