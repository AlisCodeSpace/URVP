using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Base validator class for challenge-related validations to eliminate duplicate logic
    /// </summary>
    public abstract class BaseChallengeValidator<T> : AbstractValidator<T>
    {
        protected readonly IChallengeRepository _repository;
        protected readonly ChallengeBusinessRulesValidator _businessRulesValidator;

        protected BaseChallengeValidator(IChallengeRepository repository)
        {
            _repository = repository;
            _businessRulesValidator = new ChallengeBusinessRulesValidator(repository);
        }

        /// <summary>
        /// Applies standard title validation rules
        /// </summary>
        protected void ApplyTitleValidation()
        {
            RuleFor(x => GetTitle(x))
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(ChallengeValidationConstants.TITLE_MAX_LENGTH)
                .WithMessage($"Title cannot exceed {ChallengeValidationConstants.TITLE_MAX_LENGTH} characters");
        }

        /// <summary>
        /// Applies standard description validation rules
        /// </summary>
        protected void ApplyDescriptionValidation()
        {
            RuleFor(x => GetDescription(x))
                .MaximumLength(ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH)
                .WithMessage($"Description cannot exceed {ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH} characters");
        }

        /// <summary>
        /// Applies standard estimated cost validation rules
        /// </summary>
        protected void ApplyEstimatedCostValidation()
        {
            RuleFor(x => GetEstimatedCost(x))
                .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative");
        }

        /// <summary>
        /// Applies business rules estimated cost constraints (synchronous validation only)
        /// </summary>
        protected void ApplyBusinessRulesEstimatedCostValidation()
        {
            RuleFor(x => x)
                .Must(command => 
                {
                    var costResult = _businessRulesValidator.ValidateEstimatedCostConstraints(
                        GetEstimatedCost(command));
                    return costResult.IsValid;
                })
                .WithMessage("Estimated cost constraints validation failed")
                .When(x => GetEstimatedCost(x) > 0);
        }

        /// <summary>
        /// Applies research field validation (synchronous validation only)
        /// </summary>
        protected void ApplyResearchFieldValidation()
        {
            RuleFor(x => GetResearchFieldId(x))
                .NotEmpty().WithMessage("Research field is required");
        }

        /// <summary>
        /// Applies supporting document validation
        /// </summary>
        protected void ApplySupportingDocumentValidation()
        {
            RuleFor(x => GetSupportingDocumentUrl(x))
                .MaximumLength(ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH)
                .WithMessage($"Supporting document URL cannot exceed {ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH} characters")
                .Must(url => string.IsNullOrEmpty(url) || url.EndsWith(ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"Supporting document must be a {ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION} file");
        }

        /// <summary>
        /// Applies duplicate prevention validation (synchronous validation only)
        /// Note: Async validation is handled in the service layer
        /// </summary>
        protected void ApplyDuplicatePreventionValidation()
        {
            // Async validation is handled in the service layer
            // This method is kept for consistency but doesn't add validation rules
        }

        /// <summary>
        /// Applies update restrictions validation (synchronous validation only)
        /// Note: Async validation is handled in the service layer
        /// </summary>
        protected void ApplyUpdateRestrictionsValidation()
        {
            // Async validation is handled in the service layer
            // This method is kept for consistency but doesn't add validation rules
        }

        // Abstract methods that derived classes must implement to extract values from their specific type
        protected abstract string GetTitle(T obj);
        protected abstract string? GetDescription(T obj);
        protected abstract decimal GetEstimatedCost(T obj);
        protected abstract Guid GetResearchFieldId(T obj);
        protected abstract string? GetSupportingDocumentUrl(T obj);
        protected abstract Guid GetSubmittedBy(T obj);
        protected abstract Guid GetUpdatedBy(T obj);
        protected abstract Guid GetChallengeId(T obj);
        protected abstract Guid? GetExcludeChallengeId(T obj);
    }
}
