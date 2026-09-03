using AUB.MimeDetective;
using FEA.URVP.Application.Files;
using FEA.URVP.Application.Options;

namespace FEA.URVP.Tests.Files;

public sealed class FileUploadRulesTests
{
    private static readonly FileStorageOptions Options = new()
    {
        MaxImageSizeBytes = 2_097_152,
        MaxPdfSizeBytes = 10_485_760,
        MaxTotalSizeBytes = 26_214_400,
    };

    [Fact]
    public void EnsureAccepted_valid_pdf_passes()
    {
        FileUploadRules.EnsureAccepted(MimeTypes.PDF.Mime, "transcript.pdf", 1024, image: false, Options);
    }

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.gif", "image/gif")]
    public void EnsureAccepted_valid_image_passes(string fileName, string mime)
    {
        FileUploadRules.EnsureAccepted(mime, fileName, 1024, image: true, Options);
    }

    [Fact]
    public void EnsureAccepted_valid_signature_with_invalid_extension_is_rejected()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(MimeTypes.JPEG.Mime, "photo.bmp", 1024, image: true, Options));

        Assert.Contains("JPG", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureAccepted_pdf_renamed_to_jpg_is_rejected_for_images()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(MimeTypes.PDF.Mime, "photo.jpg", 1024, image: true, Options));
    }

    [Fact]
    public void EnsureAccepted_jpeg_renamed_to_pdf_is_rejected_for_documents()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(MimeTypes.JPEG.Mime, "document.pdf", 1024, image: false, Options));
    }

    [Fact]
    public void EnsureAccepted_exe_renamed_to_pdf_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(MimeTypes.DLL_EXE.Mime, "payload.pdf", 1024, image: false, Options));
    }

    [Fact]
    public void EnsureAccepted_unknown_binary_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(null, "file.pdf", 1024, image: false, Options));
    }

    [Fact]
    public void EnsureAccepted_empty_file_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(MimeTypes.PDF.Mime, "transcript.pdf", 0, image: false, Options));
    }

    [Fact]
    public void EnsureAccepted_svg_and_office_extensions_are_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted("image/svg+xml", "icon.svg", 1024, image: true, Options));
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted("text/plain", "notes.txt", 1024, image: false, Options));
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted("application/msword", "cv.doc", 1024, image: false, Options));
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "cv.docx",
                1024,
                image: false,
                Options));
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted("image/webp", "poster.webp", 1024, image: true, Options));
    }

    [Fact]
    public void EnsureAccepted_oversized_individual_file_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(
                MimeTypes.JPEG.Mime,
                "poster.jpg",
                Options.MaxImageSizeBytes + 1,
                image: true,
                Options));

        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(
                MimeTypes.PDF.Mime,
                "transcript.pdf",
                Options.MaxPdfSizeBytes + 1,
                image: false,
                Options));
    }

    [Fact]
    public void EnsureAccepted_empty_collection_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(
                Array.Empty<(string?, string, long)>(),
                image: false,
                Options));
    }

    [Fact]
    public void EnsureAccepted_multi_file_total_size_violation_is_rejected()
    {
        var files = new (string?, string, long)[]
        {
            (MimeTypes.PDF.Mime, "a.pdf", Options.MaxPdfSizeBytes),
            (MimeTypes.PDF.Mime, "b.pdf", Options.MaxPdfSizeBytes),
            (MimeTypes.PDF.Mime, "c.pdf", Options.MaxPdfSizeBytes),
        };

        var ex = Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(files, image: false, Options));

        Assert.Contains("Total upload size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureAccepted_null_or_empty_file_in_collection_is_rejected()
    {
        var files = new (string?, string, long)[]
        {
            (MimeTypes.PDF.Mime, "ok.pdf", 100),
            (MimeTypes.PDF.Mime, "empty.pdf", 0),
        };

        Assert.Throws<ArgumentException>(() =>
            FileUploadRules.EnsureAccepted(files, image: false, Options));
    }
}
