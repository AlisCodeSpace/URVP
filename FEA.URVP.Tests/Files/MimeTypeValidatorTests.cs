using AUB.MimeDetective;
using FEA.URVP.Application.Files;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace FEA.URVP.Tests.Files;

public sealed class MimeTypeValidatorTests
{
    private readonly MimeTypeValidator _validator = new();

    [Fact]
    public async Task DetectMimeTypeAsync_valid_pdf_returns_application_pdf()
    {
        var file = FormFileFactory.Create(SampleFiles.Pdf, "transcript.pdf", "text/plain");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.PDF.Mime, mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_valid_jpeg_returns_image_jpeg()
    {
        var file = FormFileFactory.Create(SampleFiles.Jpeg, "poster.jpg");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.JPEG.Mime, mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_valid_png_returns_image_png()
    {
        var file = FormFileFactory.Create(SampleFiles.Png, "poster.png");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.PNG.Mime, mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_valid_gif_returns_image_gif()
    {
        var file = FormFileFactory.Create(SampleFiles.Gif, "poster.gif");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.GIF.Mime, mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_empty_file_returns_null()
    {
        var file = FormFileFactory.Create([], "empty.pdf");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Null(mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_null_file_returns_null()
    {
        var mime = await _validator.DetectMimeTypeAsync(null!);

        Assert.Null(mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_unknown_binary_is_not_an_allowed_type()
    {
        var file = FormFileFactory.Create(SampleFiles.UnknownBinary, "blob.bin");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.False(await _validator.IsPdfAsync(file));
        Assert.False(await _validator.IsImageAsync(file));
        Assert.False(string.Equals(mime, MimeTypes.PDF.Mime, StringComparison.OrdinalIgnoreCase));
        Assert.False(string.Equals(mime, MimeTypes.JPEG.Mime, StringComparison.OrdinalIgnoreCase));
        Assert.False(string.Equals(mime, MimeTypes.PNG.Mime, StringComparison.OrdinalIgnoreCase));
        Assert.False(string.Equals(mime, MimeTypes.GIF.Mime, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DetectMimeTypeAsync_exe_renamed_to_pdf_is_not_pdf()
    {
        var file = FormFileFactory.Create(SampleFiles.Exe, "payload.pdf", "application/pdf");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.NotEqual(MimeTypes.PDF.Mime, mime, StringComparer.OrdinalIgnoreCase);
        Assert.False(await _validator.IsPdfAsync(file));
        Assert.Equal(MimeTypes.DLL_EXE.Mime, mime);
    }

    [Fact]
    public async Task DetectMimeTypeAsync_pdf_renamed_to_jpg_still_detects_pdf()
    {
        var file = FormFileFactory.Create(SampleFiles.Pdf, "photo.jpg", "image/jpeg");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.PDF.Mime, mime);
        Assert.True(await _validator.IsPdfAsync(file));
        Assert.False(await _validator.IsImageAsync(file));
    }

    [Fact]
    public async Task DetectMimeTypeAsync_jpeg_renamed_to_pdf_still_detects_jpeg()
    {
        var file = FormFileFactory.Create(SampleFiles.Jpeg, "document.pdf", "application/pdf");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.JPEG.Mime, mime);
        Assert.True(await _validator.IsImageAsync(file));
        Assert.False(await _validator.IsPdfAsync(file));
    }

    [Fact]
    public async Task DetectMimeTypeAsync_ignores_client_content_type()
    {
        var file = FormFileFactory.Create(SampleFiles.Png, "poster.png", "application/pdf");

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Equal(MimeTypes.PNG.Mime, mime);
        Assert.NotEqual(file.ContentType, mime, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateMimeTypeAsync_compares_detected_mime_case_insensitively()
    {
        var file = FormFileFactory.Create(SampleFiles.Pdf, "transcript.pdf");

        Assert.True(await _validator.ValidateMimeTypeAsync(file, "APPLICATION/PDF"));
        Assert.False(await _validator.ValidateMimeTypeAsync(file, MimeTypes.JPEG.Mime));
    }

    [Fact]
    public async Task DetectMimeTypeAsync_open_stream_exception_returns_null()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(4);
        file.FileName.Returns("broken.pdf");
        file.OpenReadStream().Returns(_ => throw new IOException("The temporary directory is unavailable."));

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Null(mime);
        Assert.False(await _validator.IsPdfAsync(file));
        Assert.False(await _validator.IsImageAsync(file));
    }

    [Fact]
    public async Task DetectMimeTypeAsync_non_seekable_stream_returns_null()
    {
        var inner = new MemoryStream(SampleFiles.Pdf);
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(inner.Length);
        file.FileName.Returns("transcript.pdf");
        file.OpenReadStream().Returns(_ => new NonSeekableStream(inner.ToArray()));

        var mime = await _validator.DetectMimeTypeAsync(file);

        Assert.Null(mime);
    }

    private sealed class NonSeekableStream : Stream
    {
        private readonly MemoryStream _inner;

        public NonSeekableStream(byte[] content)
        {
            _inner = new MemoryStream(content);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
