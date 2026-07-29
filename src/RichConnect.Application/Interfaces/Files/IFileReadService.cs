namespace RICHConnect.Backend.Application.Interfaces.Files
{
    /// <summary>
    /// Service for reading files from database storage with authorization checks
    /// Phase 4: Database-backed file reads with streaming and caching
    /// </summary>
    public interface IFileReadService
    {
        /// <summary>
        /// Gets a file from database storage by its ID
        /// </summary>
        /// <param name="fileId">The unique identifier of the file</param>
        /// <returns>File data with metadata, or null if not found</returns>
        Task<FileReadResult?> GetFileByIdAsync(Guid fileId);

        /// <summary>
        /// Checks if a user has permission to access a file
        /// </summary>
        /// <param name="fileId">The file ID to check</param>
        /// <param name="userId">The user requesting access</param>
        /// <param name="userRoles">The roles of the user</param>
        /// <returns>True if authorized, false otherwise</returns>
        Task<bool> CanUserAccessFileAsync(Guid fileId, Guid? userId, IEnumerable<string> userRoles);

        /// <summary>
        /// Gets file metadata without loading content (for quick lookups)
        /// </summary>
        /// <param name="fileId">The file ID</param>
        /// <returns>File metadata, or null if not found</returns>
        Task<FileMetadata?> GetFileMetadataAsync(Guid fileId);

        /// <summary>
        /// Gets file ID from FileStorage by EntityType, EntityId, and FileCategory
        /// </summary>
        /// <param name="entityType">The entity type (Challenge, Partner, Theme, ResearchField)</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="fileCategory">The file category (SupportingDocument, Logo, Image, Document)</param>
        /// <returns>File ID (Guid) if found, null otherwise</returns>
        Task<Guid?> GetFileIdByEntityAsync(string entityType, Guid entityId, string fileCategory);

        /// <summary>
        /// Gets all file metadata for an entity, optionally filtered by file category
        /// </summary>
        /// <param name="entityType">The entity type (Challenge, Partner, Theme, ResearchField)</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="fileCategory">Optional file category filter (SupportingDocument, Logo, Image, Document). If null, returns all files.</param>
        /// <returns>List of file metadata</returns>
        Task<List<FileMetadata>> GetFilesByEntityAsync(string entityType, Guid entityId, string? fileCategory = null);

        /// <summary>
        /// Batch retrieves file IDs for multiple entities of the same type and category
        /// Returns a dictionary mapping entity ID to file ID (null if no file found)
        /// </summary>
        /// <param name="entityType">The entity type (Challenge, Partner, Theme, ResearchField)</param>
        /// <param name="entityIds">The list of entity IDs to lookup</param>
        /// <param name="fileCategory">The file category (SupportingDocument, Logo, Image, Document)</param>
        /// <returns>Dictionary of EntityId -> FileId (or null)</returns>
        Task<Dictionary<Guid, Guid?>> GetFileIdsByEntitiesAsync(string entityType, List<Guid> entityIds, string fileCategory);
    }

    /// <summary>
    /// Result of a file read operation including content and metadata
    /// </summary>
    public class FileReadResult
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string FileCategory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// File metadata without content (for lightweight operations)
    /// </summary>
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string FileCategory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] ContentHash { get; set; } = Array.Empty<byte>();
        public DateTime UploadedAt { get; set; }
    }
}

