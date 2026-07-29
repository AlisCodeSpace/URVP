using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Themes.SubmitTheme
{
    public class SubmitThemeCommandValidator : AbstractValidator<SubmitThemeCommand>
    {
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };
        private const int MaxImageSizeInMb = 5;
        private const int MaxDocumentSizeInMb = 10;

        public SubmitThemeCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Theme title is required.")
                .MaximumLength(128).WithMessage("Theme title cannot exceed 128 characters.");

            RuleFor(x => x.SubmittedBy)
                .NotEmpty().WithMessage("Submitted by is required.")
                .NotEqual(Guid.Empty).WithMessage("A valid submitter ID is required.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ExpectedOutcomes)
                .MaximumLength(2000).WithMessage("Expected outcomes cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ExpectedOutcomes));

            RuleFor(x => x.EstimatedFunding)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated funding must be a non-negative number.");

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
