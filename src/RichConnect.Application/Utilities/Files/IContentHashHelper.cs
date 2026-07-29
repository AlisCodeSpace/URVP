namespace RICHConnect.Backend.Application.Utilities.Files
{
    /// <summary>
    /// Interface for computing content hashes
    /// </summary>
    public interface IContentHashHelper
    {
        /// <summary>
        /// Computes SHA-256 hash of file content
        /// </summary>
        /// <param name="file">The file to hash</param>
        /// <returns>SHA-256 hash as byte array (32 bytes)</returns>
        Task<byte[]> ComputeSha256HashAsync(IFormFile file);

        /// <summary>
        /// Computes SHA-256 hash of byte array content
        /// </summary>
        /// <param name="content">The content to hash</param>
        /// <returns>SHA-256 hash as byte array (32 bytes)</returns>
        byte[] ComputeSha256Hash(byte[] content);

        /// <summary>
        /// Converts hash bytes to hex string for display/comparison
        /// </summary>
        /// <param name="hash">The hash bytes</param>
        /// <returns>Hex string representation</returns>
        string ToHexString(byte[] hash);

        /// <summary>
        /// Converts hash bytes to Base64 string for ETag headers
        /// </summary>
        /// <param name="hash">The hash bytes</param>
        /// <returns>Base64 string representation</returns>
        string ToBase64String(byte[] hash);
    }
}

