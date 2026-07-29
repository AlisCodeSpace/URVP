namespace RICHConnect.Backend.Application.Utilities.Files
{
    /// <summary>
    /// Interface for validating file MIME types using magic bytes
    /// </summary>
    public interface IMimeTypeValidator
    {
        /// <summary>
        /// Validates that a file's magic bytes match its claimed MIME type
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <param name="expectedMimeTypes">Expected MIME types</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateMimeTypeAsync(IFormFile file, params string[] expectedMimeTypes);

        /// <summary>
        /// Detects the actual MIME type of a file by reading its magic bytes
        /// </summary>
        /// <param name="file">The file to analyze</param>
        /// <returns>The detected MIME type or null if unknown</returns>
        Task<string?> DetectMimeTypeAsync(IFormFile file);

        /// <summary>
        /// Validates if a file is a valid PDF by checking magic bytes
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid PDF, false otherwise</returns>
        Task<bool> IsPdfAsync(IFormFile file);

        /// <summary>
        /// Validates if a file is a valid image by checking magic bytes
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid image, false otherwise</returns>
        Task<bool> IsImageAsync(IFormFile file);
    }
}

