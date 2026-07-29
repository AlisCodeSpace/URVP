using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for creating new challenges
    /// </summary>
    public class CreateChallengeDtoValidator : BaseChallengeValidator<CreateChallengeDto>
    {
        public CreateChallengeDtoValidator(IChallengeRepository repository) : base(repository)
        {
            // Apply standard validations
            ApplyTitleValidation();
            ApplyDescriptionValidation();
            ApplyEstimatedCostValidation();
            // Don't apply standard research field validation, use custom logic below
            ApplySupportingDocumentValidation();

            // Custom research field validation to handle "Other" option
            // ResearchFieldId is required only if OtherResearchFieldName is not provided
            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required")
                .When(x => string.IsNullOrWhiteSpace(x.OtherResearchFieldName));

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

            // Apply business rules
            ApplyBusinessRulesEstimatedCostValidation();
            ApplyDuplicatePreventionValidation();
        }

        protected override string GetTitle(CreateChallengeDto obj) => obj.Title;
        protected override string? GetDescription(CreateChallengeDto obj) => obj.Description;
        protected override decimal GetEstimatedCost(CreateChallengeDto obj) => obj.EstimatedCost;
        protected override Guid GetResearchFieldId(CreateChallengeDto obj) => obj.ResearchFieldId;
        protected override string? GetSupportingDocumentUrl(CreateChallengeDto obj) => obj.SupportingDocumentUrl;
        protected override Guid GetSubmittedBy(CreateChallengeDto obj) => Guid.Empty; // Will be set by command
        protected override Guid GetUpdatedBy(CreateChallengeDto obj) => Guid.Empty; // Not used for creation
        protected override Guid GetChallengeId(CreateChallengeDto obj) => Guid.Empty; // Not used for creation
        protected override Guid? GetExcludeChallengeId(CreateChallengeDto obj) => null; // Not used for creation
    }
}
