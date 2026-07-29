using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Validators.ResearchFields
{
    /// <summary>
    /// Validator for base ResearchField data (used for responses)
    /// </summary>
    public class ResearchFieldDtoValidator : AbstractValidator<ResearchFieldDto>
    {
        public ResearchFieldDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Research field name is required.")
                .MaximumLength(128).WithMessage("Research field name cannot exceed 128 characters.");

            RuleFor(x => x.Slug)
                .MaximumLength(128).WithMessage("Research field slug cannot exceed 128 characters.")
                .Matches(@"^[a-z0-9-]+$").When(x => !string.IsNullOrEmpty(x.Slug))
                .WithMessage("Research field slug can only contain lowercase letters, numbers, and hyphens.");

            RuleFor(x => x.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.");
                
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }

    /// <summary>
    /// Validator for creating a new ResearchField
    /// </summary>
    public class CreateResearchFieldDtoValidator : AbstractValidator<CreateResearchFieldDto>
    {
        public CreateResearchFieldDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Research field name is required.")
                .MaximumLength(128).WithMessage("Research field name cannot exceed 128 characters.");

            RuleFor(x => x.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.");
                
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }

    /// <summary>
    /// Validator for updating an existing ResearchField
    /// </summary>
    public class UpdateResearchFieldDtoValidator : AbstractValidator<UpdateResearchFieldDto>
    {
        public UpdateResearchFieldDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Research field name is required.")
                .MaximumLength(128).WithMessage("Research field name cannot exceed 128 characters.");

            RuleFor(x => x.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.");
                
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }

    /// <summary>
    /// Validator for facultySpecialist research field submission
    /// </summary>
    public class FacultySpecialistResearchFieldSubmissionDtoValidator : AbstractValidator<FacultySpecialistResearchFieldSubmissionDto>
    {
        public FacultySpecialistResearchFieldSubmissionDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Research field name is required.")
                .MaximumLength(128).WithMessage("Research field name cannot exceed 128 characters.");

            RuleFor(x => x.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.");
        }
    }
}
