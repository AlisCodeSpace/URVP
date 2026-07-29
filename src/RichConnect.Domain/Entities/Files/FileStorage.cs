using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Domain.Entities.Files
{
    /// <summary>
    /// Represents a file stored in the database
    /// </summary>
    public class FileStorage
    {
        /// <summary>
        /// Unique identifier for the file
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Type of entity this file belongs to (Challenge, Partner, Theme, ResearchField)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// ID of the entity this file belongs to
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Category of the file (SupportingDocument, Logo, Image, Document)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FileCategory { get; set; } = string.Empty;

        /// <summary>
        /// Original filename
        /// </summary>
        [Required]
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME type of the file
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        [Required]
        public long FileSize { get; set; }

        /// <summary>
        /// SHA-256 hash of the file content for integrity verification
        /// </summary>
        [Required]
        [MaxLength(32)]
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Binary content of the file
        /// </summary>
        [Required]
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// ID of the user who uploaded the file
        /// </summary>
        public Guid? UploadedBy { get; set; }

        /// <summary>
        /// Timestamp when the file was uploaded
        /// </summary>
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Soft delete flag
        /// </summary>
        [Required]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Optional security tag for classification (Public, Private, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? SecurityTag { get; set; }
    }
}

