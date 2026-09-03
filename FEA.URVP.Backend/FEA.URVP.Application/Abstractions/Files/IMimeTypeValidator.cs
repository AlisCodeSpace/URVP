using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Application.Abstractions.Files;

/// <summary>
/// Detects file MIME types from magic bytes. Does not trust
/// <see cref="IFormFile.ContentType"/> and is not a malware scanner.
/// </summary>
public interface IMimeTypeValidator
{
    Task<string?> DetectMimeTypeAsync(IFormFile file);

    Task<bool> ValidateMimeTypeAsync(IFormFile file, params string[] expectedMimeTypes);

    Task<bool> IsPdfAsync(IFormFile file);

    Task<bool> IsImageAsync(IFormFile file);
}
