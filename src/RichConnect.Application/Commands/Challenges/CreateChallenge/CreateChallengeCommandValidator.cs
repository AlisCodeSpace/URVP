using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.Validators.Challenges;

namespace RICHConnect.Backend.Application.Commands.CreateChallenge
{
    public class CreateChallengeCommandValidator : AbstractValidator<CreateChallengeCommand>
    {
        private readonly IChallengeRepository _repository;
        private readonly ChallengeBusinessRulesValidator _businessRulesValidator;

        public CreateChallengeCommandValidator(IChallengeRepository repository, ChallengeBusinessRulesValidator businessRulesValidator)
        {
            _repository = repository;
            _businessRulesValidator = businessRulesValidator;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            // ResearchFieldId is required only if OtherResearchFieldName is not provided
            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required")
                .When(x => string.IsNullOrWhiteSpace(x.OtherResearchFieldName))
                .MustAsync(async (id, cancellation) => await _repository.ValidateResearchFieldExistsAsync(id))
                .WithMessage("The selected research field does not exist or is not approved")
                .When(x => string.IsNullOrWhiteSpace(x.OtherResearchFieldName) && x.ResearchFieldId != Guid.Empty);

            // OtherResearchFieldName validation
            RuleFor(x => x.OtherResearchFieldName)
                .NotEmpty().WithMessage("Research field name is required when 'Other' is selected")
                .When(x => x.ResearchFieldId == Guid.Empty)
                .MinimumLength(3).WithMessage("Research field name must be at least 3 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.OtherResearchFieldName))
                .MaximumLength(128).WithMessage("Research field name cannot exceed 128 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.OtherResearchFieldName));

            // Either ResearchFieldId or OtherResearchFieldName must be provided
            RuleFor(x => x)
                .Must(x => x.ResearchFieldId != Guid.Empty || !string.IsNullOrWhiteSpace(x.OtherResearchFieldName))
                .WithMessage("Either select a research field or specify a custom research field name");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative");

            RuleFor(x => x.SubmittedBy)
                .NotEmpty().WithMessage("Submitted by is required");

            // Business Rules Validation
            RuleFor(x => x)
                .MustAsync((command, cancellation) => 
                {
                    var costResult = _businessRulesValidator.ValidateEstimatedCostConstraints(
                        command.EstimatedCost);
                    return Task.FromResult(costResult.IsValid);
                })
                .WithMessage("Estimated cost constraints validation failed")
                .When(x => x.EstimatedCost > 0);

            RuleFor(x => x)
                .MustAsync(async (command, cancellation) => 
                {
                    var duplicateResult = await _businessRulesValidator.ValidateDuplicatePreventionAsync(
                        command.Title, command.ResearchFieldId, command.SubmittedBy);
                    return duplicateResult.IsValid;
                })
                .WithMessage("Duplicate challenge prevention validation failed");
        }
    }
}
