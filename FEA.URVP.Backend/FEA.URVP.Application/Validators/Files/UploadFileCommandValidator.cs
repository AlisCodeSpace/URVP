using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Catalog;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Application.Validators.Files;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IOptions<FileStorageOptions> fileStorage)
    {
        var options = fileStorage.Value;

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

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File.FileName)
                .NotEmpty()
                .MaximumLength(260);

            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("File content is required.");

            When(x => FileStorageCatalog.IsImageCategory(x.FileCategory), () =>
            {
                RuleFor(x => x.File.FileName)
                    .Must(name => FileStorageCatalog.AllowedImageExtensions.Contains(Path.GetExtension(name)))
                    .WithMessage("Only JPG, JPEG, PNG, or GIF files are allowed.");

                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(options.MaxImageSizeBytes)
                    .WithMessage($"Image size must not exceed {options.MaxImageSizeBytes / (1024 * 1024)} MB.");
            }).Otherwise(() =>
            {
                RuleFor(x => x.File.FileName)
                    .Must(name => FileStorageCatalog.AllowedPdfExtensions.Contains(Path.GetExtension(name)))
                    .WithMessage("Only PDF files are allowed.");

                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(options.MaxPdfSizeBytes)
                    .WithMessage($"File size must not exceed {options.MaxPdfSizeBytes / (1024 * 1024)} MB.");
            });
        });
    }
}
