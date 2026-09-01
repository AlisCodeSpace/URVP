using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Domain.Catalog;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Files;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .Must(FileStorageCatalog.EntityTypes.Contains)
            .WithMessage("Entity type is not allowed.");

        RuleFor(x => x.EntityId)
            .NotEmpty();

        RuleFor(x => x.FileCategory)
            .NotEmpty()
            .Must(category =>
                FileStorageCatalog.DocumentCategories.Contains(category)
                || FileStorageCatalog.ImageCategories.Contains(category))
            .WithMessage("File category is not allowed.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260);

        When(x => FileStorageCatalog.IsImageCategory(x.FileCategory), () =>
        {
            RuleFor(x => x.FileName)
                .Must(name => FileStorageCatalog.AllowedImageExtensions.Contains(Path.GetExtension(name)))
                .WithMessage("Only JPG, PNG, or WebP images are allowed.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("File content is required.")
                .Must(content => content.LongLength <= FileStorageCatalog.MaxImageBytes)
                .WithMessage($"Image size must not exceed {FileStorageCatalog.MaxImageBytes / (1024 * 1024)} MB.");

            RuleFor(x => x.ContentType)
                .Must(mime =>
                    string.IsNullOrWhiteSpace(mime)
                    || FileStorageCatalog.AllowedImageMimeTypes.Contains(mime)
                    || mime.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                .WithMessage("MIME type must be an allowed image type.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.FileName)
                .Must(name => FileStorageCatalog.AllowedPdfExtensions.Contains(Path.GetExtension(name)))
                .WithMessage("Only PDF files are allowed.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("File content is required.")
                .Must(content => content.LongLength <= FileStorageCatalog.MaxDocumentBytes)
                .WithMessage($"File size must not exceed {FileStorageCatalog.MaxDocumentBytes / (1024 * 1024)} MB.");

            RuleFor(x => x.ContentType)
                .Must(mime =>
                    string.IsNullOrWhiteSpace(mime)
                    || FileStorageCatalog.AllowedPdfMimeTypes.Contains(mime)
                    || mime.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                .WithMessage("MIME type must be application/pdf.");
        });
    }
}
