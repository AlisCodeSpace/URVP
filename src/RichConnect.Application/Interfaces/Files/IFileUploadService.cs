namespace RICHConnect.Backend.Application.Interfaces.Files
{
    /// <summary>
    /// Service for file upload operations
    /// Phase 6: Extended with generic upload method for all entity types
    /// Phase 7: Added multi-file upload support
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Uploads a supporting document for a challenge
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="challengeId">The challenge ID</param>
        /// <param name="uploadedBy">Optional: User ID who uploaded the file</param>
        /// <returns>The file URL path</returns>
        Task<string> UploadSupportingDocumentAsync(IFormFile file, string challengeId, Guid? uploadedBy = null);
        
        /// <summary>
        /// Uploads multiple supporting documents for a challenge
        /// </summary>
        /// <param name="files">The files to upload</param>
        /// <param name="challengeId">The challenge ID</param>
        /// <param name="uploadedBy">Optional: User ID who uploaded the file</param>
        /// <returns>List of file IDs</returns>
        Task<List<string>> UploadMultipleSupportingDocumentsAsync(IEnumerable<IFormFile> files, string challengeId, Guid? uploadedBy = null);
        
        /// <summary>
        /// Uploads a file for any entity type (Phase 6: Generic method)
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="entityType">Entity type (Challenge, Partner, Theme, ResearchField)</param>
        /// <param name="entityId">Entity ID</param>
        /// <param name="fileCategory">File category (SupportingDocument, Logo, Image, Document)</param>
        /// <param name="uploadedBy">Optional: User ID who uploaded the file</param>
        /// <returns>The FileStorage ID as a string</returns>
        Task<string> UploadFileAsync(IFormFile file, string entityType, Guid entityId, string fileCategory, Guid? uploadedBy = null);
        
        /// <summary>
        /// Uploads multiple files for any entity type
        /// </summary>
        /// <param name="files">The files to upload</param>
        /// <param name="entityType">Entity type (Challenge, Partner, Theme, ResearchField)</param>
        /// <param name="entityId">Entity ID</param>
        /// <param name="fileCategory">File category (SupportingDocument, Logo, Image, Document)</param>
        /// <param name="uploadedBy">Optional: User ID who uploaded the files</param>
        /// <returns>List of FileStorage IDs</returns>
        Task<List<string>> UploadMultipleFilesAsync(IEnumerable<IFormFile> files, string entityType, Guid entityId, string fileCategory, Guid? uploadedBy = null);
        
        /// <summary>
        /// Validates a file for upload
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateFileAsync(IFormFile file);
        
        /// <summary>
        /// Validates multiple files for upload with total size check
        /// </summary>
        /// <param name="files">The files to validate</param>
        /// <param name="maxTotalSizeBytes">Maximum total size for all files combined (optional, defaults to config)</param>
        /// <returns>True if all files are valid and total size is within limit</returns>
        Task<(bool isValid, string? errorMessage)> ValidateMultipleFilesAsync(IEnumerable<IFormFile> files, long? maxTotalSizeBytes = null);
        
        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="filePath">The file path to delete</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteFileAsync(string filePath);
        
        /// <summary>
        /// Gets the maximum file size in bytes
        /// </summary>
        /// <returns>Maximum file size in bytes</returns>
        long GetMaxFileSize();
        
        /// <summary>
        /// Gets the allowed file extensions
        /// </summary>
        /// <returns>Array of allowed extensions</returns>
        string[] GetAllowedExtensions();
        
        /// <summary>
        /// Updates the EntityId of a file (used when entity is created after file upload)
        /// </summary>
        /// <param name="fileId">The file ID</param>
        /// <param name="entityId">The new entity ID</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateFileEntityIdAsync(Guid fileId, Guid entityId);
    }
}
