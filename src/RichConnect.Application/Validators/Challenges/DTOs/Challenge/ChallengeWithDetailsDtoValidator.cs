using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for detailed challenge views
    /// </summary>
    public class ChallengeWithDetailsDtoValidator : AbstractValidator<ChallengeWithDetailsDto>
    {
        public ChallengeWithDetailsDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Challenge ID is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(ChallengeValidationConstants.TITLE_MAX_LENGTH)
                .WithMessage($"Title cannot exceed {ChallengeValidationConstants.TITLE_MAX_LENGTH} characters");

            RuleFor(x => x.Description)
                .MaximumLength(ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH)
                .WithMessage($"Description cannot exceed {ChallengeValidationConstants.DESCRIPTION_MAX_LENGTH} characters");

            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field ID is required");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative");

            RuleFor(x => x.SupportingDocumentUrl)
                .MaximumLength(ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH)
                .WithMessage($"Supporting document URL cannot exceed {ChallengeValidationConstants.SUPPORTING_DOCUMENT_URL_MAX_LENGTH} characters")
                .Must(url => string.IsNullOrEmpty(url) || url.EndsWith(ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"Supporting document must be a {ChallengeValidationConstants.SUPPORTING_DOCUMENT_ALLOWED_EXTENSION} file");

            RuleFor(x => x.SubmittedBy)
                .NotEmpty().WithMessage("Submitted by is required");

            RuleFor(x => x.SubmitterName)
                .NotEmpty().WithMessage("Submitter name is required");

            RuleFor(x => x.ResearchFieldName)
                .NotEmpty().WithMessage("Research field name is required");

            RuleFor(x => x.MatchedFacultySpecialistIds)
                .NotNull().WithMessage("Matched facultySpecialist IDs cannot be null");
        }
    }
}
