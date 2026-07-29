using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Validators.Themes
{
    /// <summary>
    /// Validator for base theme data (used for responses)
    /// </summary>
    public class ResearchThemeDtoValidator : AbstractValidator<ResearchThemeDto>
    {
        public ResearchThemeDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Theme title is required.")
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.");

            RuleFor(x => x.Slug)
                .MaximumLength(128).WithMessage("Theme slug cannot exceed 128 characters.")
                .Matches(@"^[a-z0-9-]+$").WithMessage("Theme slug can only contain lowercase letters, numbers, and hyphens.");

            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.");
                
            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.");
                
            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a positive number.");
        }
    }

    /// <summary>
    /// Validator for facultySpecialist theme submission
    /// </summary>
    public class FacultySpecialistResearchThemeSubmissionDtoValidator : AbstractValidator<FacultySpecialistThemeSubmissionDto>
    {
        public FacultySpecialistResearchThemeSubmissionDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Theme title is required.")
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.");
                
            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.");
                
            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a positive number.");

            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required.");

            // Document validation is handled in the controller since it's a file upload

            // When creating a new theme, ID should be empty
            RuleSet("create", () =>
            {
                RuleFor(x => x.Id).Equal(Guid.Empty).WithMessage("ID should not be provided when creating a new theme.");
            });
        }
    }

    /// <summary>
    /// Validator for admin theme creation
    /// </summary>
    public class AdminResearchThemeCreationDtoValidator : AbstractValidator<AdminThemeCreationDto>
    {
        public AdminResearchThemeCreationDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Theme title is required.")
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.");
                
            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.");
                
            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a positive number.");

            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required.");

            // Image validation is handled in the controller since it's a file upload

            // When creating a new theme, ID should be empty
            RuleSet("create", () =>
            {
                RuleFor(x => x.Id).Equal(Guid.Empty).WithMessage("ID should not be provided when creating a new theme.");
            });
        }
    }

    /// <summary>
    /// Validator for admin theme updates
    /// </summary>
    public class AdminResearchThemeUpdateDtoValidator : AbstractValidator<AdminThemeUpdateDto>
    {
        public AdminResearchThemeUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Theme title is required.")
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters.");
                
            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.");
                
            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a positive number.");

            RuleFor(x => x.ResearchFieldId)
                .NotEmpty().WithMessage("Research field is required.");

            // Image validation is handled in the controller since it's a file upload
        }
    }
}
