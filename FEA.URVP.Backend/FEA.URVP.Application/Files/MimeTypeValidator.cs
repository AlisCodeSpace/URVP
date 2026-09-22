using AUB.MimeDetective;
using FEA.URVP.Application.Abstractions.Files;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Application.Files;

/// <summary>
/// Magic-byte MIME detection via AUB.MimeDetective. Exceptions, empty files,
/// and unknown signatures are treated as invalid.
/// </summary>
public sealed class MimeTypeValidator : IMimeTypeValidator
{
    private static readonly string[] ImageMimeTypes =
    [
        MimeTypes.JPEG.Mime,
        MimeTypes.PNG.Mime,
        MimeTypes.GIF.Mime,
    ];

    public Task<string?> DetectMimeTypeAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            // IFormFile owns this stream; disposing it can prevent the handler from reading content.
            var stream = file.OpenReadStream();

            // Throws for a non-seekable stream, which is rejected rather than consumed: the
            // upload handler reads the same stream after this call and needs it rewound.
            stream.Seek(0, SeekOrigin.Begin);

            // Signatures live in the first MaxHeaderSize bytes, so only that prefix is read.
            // The Stream overload of GetFileType() spools the entire upload to a temp file on
            // disk to inspect the same prefix; the Func overload does no I/O of its own.
            // Short reads leave the tail zero-filled, matching the library's own file reader.
            var header = new byte[MimeTypes.MaxHeaderSize];
            stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
            stream.Seek(0, SeekOrigin.Begin);

            // No file name is supplied, so a ZIP container is reported as application/zip
            // rather than being opened to refine it to .docx/.xlsx. Neither is an accepted
            // upload type, so both outcomes are rejected identically downstream.
            var fileType = MimeTypes.GetFileType(() => header);

            return Task.FromResult(fileType?.Mime);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public async Task<bool> ValidateMimeTypeAsync(IFormFile file, params string[] expectedMimeTypes)
    {
        var mime = await DetectMimeTypeAsync(file);
        if (mime is null || expectedMimeTypes is null || expectedMimeTypes.Length == 0)
        {
            return false;
        }

        foreach (var expected in expectedMimeTypes)
        {
            if (string.Equals(expected, mime, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> IsPdfAsync(IFormFile file)
    {
        var mime = await DetectMimeTypeAsync(file);
        return mime is not null
            && string.Equals(mime, MimeTypes.PDF.Mime, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> IsImageAsync(IFormFile file)
    {
        var mime = await DetectMimeTypeAsync(file);
        if (mime is null)
        {
            return false;
        }

        foreach (var allowed in ImageMimeTypes)
        {
            if (string.Equals(allowed, mime, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
