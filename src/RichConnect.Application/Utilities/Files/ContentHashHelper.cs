using System.Security.Cryptography;
using System.Text;

namespace RICHConnect.Backend.Application.Utilities.Files
{
    /// <summary>
    /// Implementation for computing content hashes (SHA-256)
    /// Phase 2: Full implementation
    /// </summary>
    public class ContentHashHelper : IContentHashHelper
    {
        public async Task<byte[]> ComputeSha256HashAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty", nameof(file));

            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            
            return await sha256.ComputeHashAsync(stream);
        }

        public byte[] ComputeSha256Hash(byte[] content)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException("Content is null or empty", nameof(content));

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(content);
        }

        public string ToHexString(byte[] hash)
        {
            if (hash == null || hash.Length == 0)
                return string.Empty;

            var builder = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                builder.AppendFormat("{0:x2}", b);
            }
            return builder.ToString();
        }

        public string ToBase64String(byte[] hash)
        {
            if (hash == null || hash.Length == 0)
                return string.Empty;

            return Convert.ToBase64String(hash);
        }
    }
}

