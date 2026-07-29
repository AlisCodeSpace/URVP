using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for challenge data with rule sets for different operations
    /// </summary>
    public class ChallengeDtoValidator : AbstractValidator<ChallengeDto>
    {
        public ChallengeDtoValidator()
        {
            // Common validation rules
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Challenge title is required")
                .MaximumLength(ChallengeValidationConstants.TITLE_MAX_LENGTH)
                .WithMessage($"Challenge title cannot exceed {ChallengeValidationConstants.TITLE_MAX_LENGTH} characters");

            RuleFor(x => x.Description)
                .MaximumLength(ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH)
                .WithMessage($"Description cannot exceed {ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH} characters");

            RuleFor(x => x.ResearchFieldId)
                .NotEqual(Guid.Empty).WithMessage("A valid research field ID is required");

            // Estimated cost validation
            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be a positive number");

            // Supporting document validation
            RuleFor(x => x.SupportingDocumentUrl)
                .MaximumLength(ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH)
                .WithMessage($"Supporting document URL cannot exceed {ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH} characters")
                .Must(url => string.IsNullOrEmpty(url) || url.EndsWith(ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"Supporting document must be a {ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION} file");

            // When creating a new challenge, ID should be empty
            RuleSet("create", () =>
            {
                RuleFor(x => x.Id).Equal(Guid.Empty).WithMessage("ID should not be provided when creating a new challenge");
            });

            // When updating a challenge, ID should be valid
            RuleSet("update", () =>
            {
                RuleFor(x => x.Id).NotEqual(Guid.Empty).WithMessage("A valid challenge ID is required");
            });
            
            // When matching professors to a challenge
            RuleSet("match", () =>
            {
                RuleFor(x => x.MatchedFacultySpecialistIds)
                    .NotNull().WithMessage("facultySpecialist IDs list cannot be null")
                    .Must(ids => ids != null && ids.Any()).WithMessage("At least one facultySpecialist ID is required");
            });
        }
    }
} 