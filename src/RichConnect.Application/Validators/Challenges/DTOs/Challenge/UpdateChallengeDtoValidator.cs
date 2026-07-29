using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for updating challenge requests
    /// </summary>
    public class UpdateChallengeDtoValidator : BaseChallengeValidator<UpdateChallengeDto>
    {
        public UpdateChallengeDtoValidator(IChallengeRepository repository) : base(repository)
        {
            // Apply standard validations
            ApplyTitleValidation();
            ApplyDescriptionValidation();
            ApplyEstimatedCostValidation();
            ApplyResearchFieldValidation();
            ApplySupportingDocumentValidation();

            // Apply business rules
            ApplyBusinessRulesEstimatedCostValidation();
            ApplyDuplicatePreventionValidation();
            ApplyUpdateRestrictionsValidation();
        }

        protected override string GetTitle(UpdateChallengeDto obj) => obj.Title;
        protected override string? GetDescription(UpdateChallengeDto obj) => obj.Description;
        protected override decimal GetEstimatedCost(UpdateChallengeDto obj) => obj.EstimatedCost;
        protected override Guid GetResearchFieldId(UpdateChallengeDto obj) => obj.ResearchFieldId;
        protected override string? GetSupportingDocumentUrl(UpdateChallengeDto obj) => obj.SupportingDocumentUrl;
        protected override Guid GetSubmittedBy(UpdateChallengeDto obj) => Guid.Empty; // Not used for updates
        protected override Guid GetUpdatedBy(UpdateChallengeDto obj) => Guid.Empty; // Will be set by command
        protected override Guid GetChallengeId(UpdateChallengeDto obj) => Guid.Empty; // Will be set by command
        protected override Guid? GetExcludeChallengeId(UpdateChallengeDto obj) => null; // Will be set by command
    }
}
