using AUB.MimeDetective;
using Microsoft.AspNetCore.Http;

namespace RICHConnect.Backend.Application.Utilities.Files
{
    /// <summary>
    /// Implementation for MIME type validation using magic bytes.
    /// Uses the AUB.MimeDetective library to inspect file signatures instead of relying
    /// on Content-Type headers or file extensions.
    /// </summary>
    public class MimeTypeValidator : IMimeTypeValidator
    {
        public async Task<bool> ValidateMimeTypeAsync(IFormFile file, params string[] expectedMimeTypes)
        {
            if (file == null || file.Length == 0)
                return false;

            var detectedMimeType = await DetectMimeTypeAsync(file);
            
            if (string.IsNullOrEmpty(detectedMimeType))
                return false;

            return expectedMimeTypes.Any(expected => 
                string.Equals(expected, detectedMimeType, StringComparison.OrdinalIgnoreCase));
        }

        // Allowed image MIME types (SVG excluded for security)
        private static readonly HashSet<string> AllowedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            MimeTypes.JPEG.Mime,
            MimeTypes.PNG.Mime,
            MimeTypes.GIF.Mime
        };

        public async Task<string?> DetectMimeTypeAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                using var stream = file.OpenReadStream();

                // Use MimeDetective to infer the file type from its magic bytes.
                // This writes the stream to a temporary file internally and inspects the header,
                // matching against a rich set of known signatures.
                var fileType = stream.GetFileType();

                return fileType?.Mime;
            }
            catch
            {
                // On any failure (I/O, temp file issues, etc.), fall back to "unknown".
                return null;
            }
        }

        public async Task<bool> IsPdfAsync(IFormFile file)
        {
            var mimeType = await DetectMimeTypeAsync(file);
            return string.Equals(mimeType, MimeTypes.PDF.Mime, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> IsImageAsync(IFormFile file)
        {
            var mimeType = await DetectMimeTypeAsync(file);
            return mimeType != null && AllowedImageMimeTypes.Contains(mimeType);
        }
    }
}

