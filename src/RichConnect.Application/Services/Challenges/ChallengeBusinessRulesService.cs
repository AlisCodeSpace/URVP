using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.Validators.Challenges;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Challenges
{
    /// <summary>
    /// Service for enforcing comprehensive business rules for Challenge operations
    /// </summary>
    public class ChallengeBusinessRulesService
    {
        private readonly IChallengeRepository _repository;
        private readonly ChallengeBusinessRulesValidator _validator;

        public ChallengeBusinessRulesService(IChallengeRepository repository)
        {
            _repository = repository;
            _validator = new ChallengeBusinessRulesValidator(repository);
        }

        /// <summary>
        /// Validates all business rules for challenge creation
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateChallengeCreationAsync(
            string title, 
            Guid researchFieldId, 
            decimal estimatedCost, 
            Guid submittedBy)
        {
            var result = new BusinessRulesValidationResult();

            // Validate estimated cost constraints
            var costResult = _validator.ValidateEstimatedCostConstraints(estimatedCost);
            if (!costResult.IsValid)
            {
                result.AddErrors(costResult.Errors.Select(e => e.ErrorMessage));
            }

            // Validate duplicate prevention
            var duplicateResult = await _validator.ValidateDuplicatePreventionAsync(
                title, researchFieldId, submittedBy);
            if (!duplicateResult.IsValid)
            {
                result.AddErrors(duplicateResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }

        /// <summary>
        /// Validates all business rules for challenge updates
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateChallengeUpdateAsync(
            Guid challengeId, 
            string title, 
            Guid researchFieldId, 
            decimal estimatedCost, 
            Guid updatedBy,
            bool isAdmin = false)
        {
            var result = new BusinessRulesValidationResult();

            // Validate update restrictions
            var updateResult = await _validator.ValidateUpdateRestrictionsAsync(challengeId, updatedBy, isAdmin);
            if (!updateResult.IsValid)
            {
                result.AddErrors(updateResult.Errors.Select(e => e.ErrorMessage));
            }

            // Validate estimated cost constraints
            var costResult = _validator.ValidateEstimatedCostConstraints(estimatedCost);
            if (!costResult.IsValid)
            {
                result.AddErrors(costResult.Errors.Select(e => e.ErrorMessage));
            }

            // Validate duplicate prevention
            var duplicateResult = await _validator.ValidateDuplicatePreventionAsync(
                title, researchFieldId, updatedBy, challengeId);
            if (!duplicateResult.IsValid)
            {
                result.AddErrors(duplicateResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }

        /// <summary>
        /// Validates all business rules for challenge status transitions
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateStatusTransitionAsync(
            Guid challengeId, 
            ChallengeStatus newStatus)
        {
            var result = new BusinessRulesValidationResult();
            var challenge = await _repository.GetByIdAsync(challengeId);

            if (challenge == null)
            {
                result.AddError("Challenge not found");
                return result;
            }

            var statusResult = await _validator.ValidateStatusTransitionAsync(
                challengeId, challenge.Status, newStatus);
            if (!statusResult.IsValid)
            {
                result.AddErrors(statusResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }

        /// <summary>
        /// Validates all business rules for matching operations
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateMatchingOperationsAsync(Guid challengeId)
        {
            var result = new BusinessRulesValidationResult();

            var matchingResult = await _validator.ValidateMatchingOperationsAsync(challengeId);
            if (!matchingResult.IsValid)
            {
                result.AddErrors(matchingResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }

        /// <summary>
        /// Validates all business rules for finalization operations
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateFinalizationAsync(Guid challengeId)
        {
            var result = new BusinessRulesValidationResult();

            var finalizationResult = await _validator.ValidateFinalizationRulesAsync(challengeId);
            if (!finalizationResult.IsValid)
            {
                result.AddErrors(finalizationResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }

        /// <summary>
        /// Validates time-based rules for challenges
        /// </summary>
        public BusinessRulesValidationResult ValidateTimeBasedRules(DateTime? deadline = null)
        {
            var result = new BusinessRulesValidationResult();

            var timeResult = _validator.ValidateTimeBasedRules(deadline);
            if (!timeResult.IsValid)
            {
                result.AddErrors(timeResult.Errors.Select(e => e.ErrorMessage));
            }

            return result;
        }
    }

    /// <summary>
    /// Result of business rules validation
    /// </summary>
    public class BusinessRulesValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; private set; } = new List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }

        public string GetErrorMessage()
        {
            return string.Join("; ", Errors);
        }
    }
}
