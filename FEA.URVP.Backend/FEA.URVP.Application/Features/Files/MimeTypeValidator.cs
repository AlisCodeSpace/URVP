using AUB.MimeDetective;
using FEA.URVP.Application.Abstractions.Files;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Application.Files;

/// <summary>
/// Magic-byte MIME detection via AUB.MimeDetective. Exceptions, empty files,
/// and unknown signatures are treated as invalid. Detection reads only the
/// header the library compares (<see cref="MimeTypes.MaxHeaderSize"/> bytes)
/// and calls the <c>GetFileType</c> overload that takes a header, which does not
/// write a temporary file.
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
            // Seek first so a non-seekable stream fails closed, matching the library's stream overload.
            var stream = file.OpenReadStream();
            stream.Seek(0, SeekOrigin.Begin);

            var header = new byte[MimeTypes.MaxHeaderSize];
            var totalRead = 0;
            while (totalRead < header.Length)
            {
                var read = stream.Read(header, totalRead, header.Length - totalRead);
                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

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
