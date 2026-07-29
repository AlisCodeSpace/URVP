using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Themes.UpdateTheme
{
    public class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
    {
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
        private const int MaxImageSizeInMb = 5;
        private const int MaxDocumentSizeInMb = 10;

        public UpdateThemeCommandValidator()
        {
            RuleFor(x => x.ThemeId)
                .NotEmpty().WithMessage("Theme ID is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid theme ID is required.");

            RuleFor(x => x.UpdatedBy)
                .NotEmpty().WithMessage("Updated by is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid updater ID is required.");

            // At least one field must be provided for update
            RuleFor(x => x)
                .Must(HaveAtLeastOneUpdateField)
                .WithMessage("At least one field must be provided for update.");

            RuleFor(x => x.Title)
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ExpectedOutcomes));

            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a non-negative number.")
                .When(x => x.EstimatedFunding.HasValue);

            RuleFor(x => x.ResearchFieldId)
                .NotEqual(Guid.Empty).When(x => x.ResearchFieldId.HasValue)
                .WithMessage("A valid research field ID is required when specified.");

            // Image validation
            When(x => x.Image != null, () =>
            {
                RuleFor(x => x.Image)
                    .Must(ValidateImageFileType).WithMessage($"Image file must be one of the following types: {string.Join(", ", _allowedImageExtensions)}")
                    .Must(ValidateImageFileSize).WithMessage($"Image file size must not exceed {MaxImageSizeInMb}MB.");
            });

            // Document validation
            When(x => x.Document != null, () =>
            {
                RuleFor(x => x.Document)
                    .Must(ValidateDocumentFileType).WithMessage($"Document file must be one of the following types: {string.Join(", ", _allowedDocumentExtensions)}")
                    .Must(ValidateDocumentFileSize).WithMessage($"Document file size must not exceed {MaxDocumentSizeInMb}MB.");
            });
        }

        private bool HaveAtLeastOneUpdateField(UpdateThemeCommand command)
        {
            return !string.IsNullOrEmpty(command.Title) ||
                   !string.IsNullOrEmpty(command.Description) ||
                   !string.IsNullOrEmpty(command.ExpectedOutcomes) ||
                   command.EstimatedFunding.HasValue ||
                   command.ResearchFieldId.HasValue ||
                   command.Image != null ||
                   command.Document != null;
        }

        private bool ValidateImageFileType(IFormFile? file)
        {
            if (file == null) return true;
            
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }

        private bool ValidateImageFileSize(IFormFile? file)
        {
            if (file == null) return true;
            
            return file.Length <= MaxImageSizeInMb * 1024 * 1024;
        }

        private bool ValidateDocumentFileType(IFormFile? file)
        {
            if (file == null) return true;
            
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedDocumentExtensions.Contains(extension);
        }

        private bool ValidateDocumentFileSize(IFormFile? file)
        {
            if (file == null) return true;
            
            return file.Length <= MaxDocumentSizeInMb * 1024 * 1024;
        }
    }
}
