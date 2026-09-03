using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Validators.Files;
using FEA.URVP.Domain.Catalog;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Tests.Files;

public sealed class UploadFileCommandValidatorTests
{
    private static readonly FileStorageOptions Options = new()
    {
        MaxImageSizeBytes = 2_097_152,
        MaxPdfSizeBytes = 10_485_760,
        MaxTotalSizeBytes = 26_214_400,
    };

    private static UploadFileCommandValidator CreateValidator() =>
        new(Microsoft.Extensions.Options.Options.Create(Options));

    [Fact]
    public async Task Valid_pdf_passes_early_rules()
    {
        var command = PdfCommand("transcript.pdf", 1024);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("poster.jpg")]
    [InlineData("poster.jpeg")]
    [InlineData("poster.png")]
    [InlineData("poster.gif")]
    public async Task Valid_image_extension_passes_early_rules(string fileName)
    {
        var command = ImageCommand(fileName, 1024);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("notes.txt")]
    [InlineData("cv.doc")]
    [InlineData("cv.docx")]
    [InlineData("icon.svg")]
    [InlineData("poster.webp")]
    public async Task Disallowed_extensions_are_rejected(string fileName)
    {
        var isImage = Path.GetExtension(fileName) is ".svg" or ".webp";
        var command = isImage
            ? ImageCommand(fileName, 1024)
            : PdfCommand(fileName, 1024);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        var command = PdfCommand("transcript.pdf", 0);

        var result = await CreateValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Oversized_individual_file_is_rejected()
    {
        var pdf = PdfCommand("transcript.pdf", Options.MaxPdfSizeBytes + 1);
        var image = ImageCommand("poster.jpg", Options.MaxImageSizeBytes + 1);

        Assert.False((await CreateValidator().ValidateAsync(pdf)).IsValid);
        Assert.False((await CreateValidator().ValidateAsync(image)).IsValid);
    }

    [Fact]
    public async Task Client_content_type_is_not_used_to_authorize()
    {
        var command = PdfCommand("transcript.pdf", 1024, contentType: "application/x-msdownload");

        var result = await CreateValidator().ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    private static UploadFileCommand PdfCommand(string fileName, long length, string contentType = "application/pdf") =>
        new()
        {
            EntityType = FileStorageCatalog.EntityStudentProfile,
            EntityId = Guid.NewGuid(),
            FileCategory = FileStorageCatalog.CategoryTranscript,
            File = FormFileFactory.CreateWithDeclaredLength(fileName, length, contentType),
        };

    private static UploadFileCommand ImageCommand(string fileName, long length) =>
        new()
        {
            EntityType = FileStorageCatalog.EntityWorkshop,
            EntityId = Guid.NewGuid(),
            FileCategory = FileStorageCatalog.CategoryPoster,
            File = FormFileFactory.CreateWithDeclaredLength(fileName, length, "image/jpeg"),
        };
}
