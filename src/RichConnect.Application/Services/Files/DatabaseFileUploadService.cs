using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Utilities.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Entities.Files;

namespace RICHConnect.Backend.Application.Services.Files
{
    /// <summary>
    /// Database-backed file upload service for storing files in SQL Server
    /// Phase 6: Database-only storage (legacy file system and Azure storage removed)
    /// Phase 7: Added multi-file upload support with total size validation
    /// </summary>
    public class DatabaseFileUploadService : IFileUploadService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseFileUploadService> _logger;
        private readonly IMimeTypeValidator _mimeTypeValidator;
        private readonly IContentHashHelper _contentHashHelper;

        private const long MaxPdfSize = 25 * 1024 * 1024; // 25 MB
        private const long MaxImageSize = 25 * 1024 * 1024; // 25 MB
        private const long MaxTotalSize = 25 * 1024 * 1024; // 25 MB total for multiple files
        private static readonly string[] AllowedPdfExtensions = { ".pdf" };
        // SVG removed for security - SVG files can contain executable JavaScript when served inline
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public DatabaseFileUploadService(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<DatabaseFileUploadService> logger,
            IMimeTypeValidator mimeTypeValidator,
            IContentHashHelper contentHashHelper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mimeTypeValidator = mimeTypeValidator ?? throw new ArgumentNullException(nameof(mimeTypeValidator));
            _contentHashHelper = contentHashHelper ?? throw new ArgumentNullException(nameof(contentHashHelper));
        }

        /// <summary>
        /// Uploads a supporting document for a challenge (Phase 6: Database-only storage)
        /// </summary>
        public async Task<string> UploadSupportingDocumentAsync(IFormFile file, string challengeId, Guid? uploadedBy = null)
        {
            try
            {
                // Validate the file
                if (!await ValidateFileAsync(file))
                {
                    _logger.LogWarning("File validation failed for challenge {ChallengeId}: {FileName}", 
                        challengeId, file.FileName);
                    throw new ArgumentException("File validation failed");
                }

                // Compute content hash for integrity verification
                var contentHash = await _contentHashHelper.ComputeSha256HashAsync(file);
                
                // Read file content
                byte[] content;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    content = memoryStream.ToArray();
                }

                // Detect MIME type
                var mimeType = await _mimeTypeValidator.DetectMimeTypeAsync(file) ?? "application/octet-stream";

                // Create FileStorage entity
                // IMPORTANT: 
                // - Id = unique file identifier (GUID for the file itself)
                // - EntityId = ID of the entity this file belongs to (e.g., Challenge ID, Theme ID)
                // DO NOT swap these values!
                var fileStorage = new FileStorage
                {
                    Id = Guid.NewGuid(), // File ID - unique identifier for this file record
                    EntityType = "Challenge",
                    EntityId = Guid.Parse(challengeId), // Entity ID - the Challenge this file belongs to
                    FileCategory = "SupportingDocument",
                    FileName = file.FileName,
                    MimeType = mimeType,
                    FileSize = file.Length,
                    ContentHash = contentHash,
                    Content = content,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                // Insert into database
                await _context.FileStorage.AddAsync(fileStorage);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "File uploaded to database successfully. FileId: {FileId}, Challenge: {ChallengeId}, " +
                    "FileName: {FileName}, Size: {FileSize} bytes, Hash: {ContentHash}",
                    fileStorage.Id, challengeId, file.FileName, file.Length,
                    _contentHashHelper.ToHexString(contentHash));

                // Return the file ID as the path (for compatibility)
                return fileStorage.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to upload file to database for challenge {ChallengeId}: {FileName}", 
                    challengeId, file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Uploads multiple supporting documents for a challenge (Phase 7: Multi-file support)
        /// </summary>
        public async Task<List<string>> UploadMultipleSupportingDocumentsAsync(IEnumerable<IFormFile> files, string challengeId, Guid? uploadedBy = null)
        {
            var fileList = files.ToList();
            
            // Validate all files
            var validationResult = await ValidateMultipleFilesAsync(fileList);
            if (!validationResult.isValid)
            {
                _logger.LogWarning("Multiple files validation failed for challenge {ChallengeId}: {Error}", 
                    challengeId, validationResult.errorMessage);
                throw new ArgumentException(validationResult.errorMessage ?? "File validation failed");
            }

            var uploadedFileIds = new List<string>();
            var uploadErrors = new List<string>();

            try
            {
                foreach (var file in fileList)
                {
                    try
                    {
                        var fileId = await UploadSupportingDocumentAsync(file, challengeId, uploadedBy);
                        uploadedFileIds.Add(fileId);
                    }
                    catch (Exception ex)
                    {
                        uploadErrors.Add($"{file.FileName}: {ex.Message}");
                        _logger.LogError(ex, "Failed to upload file {FileName} for challenge {ChallengeId}", 
                            file.FileName, challengeId);
                    }
                }

                if (uploadErrors.Any())
                {
                    // Clean up successfully uploaded files if any upload failed
                    foreach (var fileId in uploadedFileIds)
                    {
                        try
                        {
                            await DeleteFileAsync(fileId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to clean up file {FileId} after upload failure", fileId);
                        }
                    }

                    throw new InvalidOperationException($"File upload failed: {string.Join("; ", uploadErrors)}");
                }

                _logger.LogInformation(
                    "Successfully uploaded {Count} files for challenge {ChallengeId}", 
                    uploadedFileIds.Count, challengeId);

                return uploadedFileIds;
            }
            catch
            {
                // Clean up any uploaded files on exception
                foreach (var fileId in uploadedFileIds)
                {
                    try
                    {
                        await DeleteFileAsync(fileId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to clean up file {FileId} after exception", fileId);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Generic file upload method for any entity type (Phase 6: Unified upload)
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string entityType, Guid entityId, string fileCategory, Guid? uploadedBy = null)
        {
            try
            {
                // Validate the file
                if (!await ValidateFileAsync(file))
                {
                    _logger.LogWarning("File validation failed for {EntityType} {EntityId}: {FileName}", 
                        entityType, entityId, file.FileName);
                    throw new ArgumentException("File validation failed");
                }

                // Compute content hash
                var contentHash = await _contentHashHelper.ComputeSha256HashAsync(file);
                
                // Read file content
                byte[] content;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    content = memoryStream.ToArray();
                }

                // Detect MIME type
                var mimeType = await _mimeTypeValidator.DetectMimeTypeAsync(file) ?? "application/octet-stream";

                // Create FileStorage entity
                // IMPORTANT: 
                // - Id = unique file identifier (GUID for the file itself)
                // - EntityId = ID of the entity this file belongs to (e.g., Theme ID, Partner ID, ResearchField ID)
                // DO NOT swap these values! The entityId parameter passed to this method is the entity ID, NOT the file ID.
                var fileStorage = new FileStorage
                {
                    Id = Guid.NewGuid(), // File ID - unique identifier for this file record
                    EntityType = entityType,
                    EntityId = entityId, // Entity ID - the entity (Theme/Partner/ResearchField/etc.) this file belongs to
                    FileCategory = fileCategory,
                    FileName = file.FileName,
                    MimeType = mimeType,
                    FileSize = file.Length,
                    ContentHash = contentHash,
                    Content = content,
                    UploadedBy = uploadedBy,
                    UploadedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                // Insert into database
                await _context.FileStorage.AddAsync(fileStorage);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "File uploaded to database successfully: FileId={FileId}, EntityType={EntityType}, EntityId={EntityId}, " +
                    "FileCategory={FileCategory}, FileName={FileName}, Size={FileSize} bytes, Hash={ContentHash}",
                    fileStorage.Id, entityType, entityId, fileCategory, file.FileName, file.Length,
                    _contentHashHelper.ToHexString(contentHash));

                // Return the file ID as the path (for compatibility)
                return fileStorage.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to upload file to database for {EntityType} {EntityId}: {FileName}", 
                    entityType, entityId, file.FileName);
                throw;
            }
        }

        /// <summary>
        /// Uploads multiple files for any entity type (Phase 7: Multi-file support)
        /// </summary>
        public async Task<List<string>> UploadMultipleFilesAsync(IEnumerable<IFormFile> files, string entityType, Guid entityId, string fileCategory, Guid? uploadedBy = null)
        {
            var fileList = files.ToList();
            
            // Validate all files
            var validationResult = await ValidateMultipleFilesAsync(fileList);
            if (!validationResult.isValid)
            {
                _logger.LogWarning("Multiple files validation failed for {EntityType} {EntityId}: {Error}", 
                    entityType, entityId, validationResult.errorMessage);
                throw new ArgumentException(validationResult.errorMessage ?? "File validation failed");
            }

            var uploadedFileIds = new List<string>();
            var uploadErrors = new List<string>();

            try
            {
                foreach (var file in fileList)
                {
                    try
                    {
                        var fileId = await UploadFileAsync(file, entityType, entityId, fileCategory, uploadedBy);
                        uploadedFileIds.Add(fileId);
                    }
                    catch (Exception ex)
                    {
                        uploadErrors.Add($"{file.FileName}: {ex.Message}");
                        _logger.LogError(ex, "Failed to upload file {FileName} for {EntityType} {EntityId}", 
                            file.FileName, entityType, entityId);
                    }
                }

                if (uploadErrors.Any())
                {
                    // Clean up successfully uploaded files if any upload failed
                    foreach (var fileId in uploadedFileIds)
                    {
                        try
                        {
                            await DeleteFileAsync(fileId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to clean up file {FileId} after upload failure", fileId);
                        }
                    }

                    throw new InvalidOperationException($"File upload failed: {string.Join("; ", uploadErrors)}");
                }

                _logger.LogInformation(
                    "Successfully uploaded {Count} files for {EntityType} {EntityId}", 
                    uploadedFileIds.Count, entityType, entityId);

                return uploadedFileIds;
            }
            catch
            {
                // Clean up any uploaded files on exception
                foreach (var fileId in uploadedFileIds)
                {
                    try
                    {
                        await DeleteFileAsync(fileId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to clean up file {FileId} after exception", fileId);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Validates a file for upload (Phase 6: Database-only storage)
        /// </summary>
        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Validation failed: File is null or empty");
                return false;
            }

            // Get size limits from configuration
            var maxPdfSize = _configuration.GetValue<long>("FileStorage:MaxPdfSizeBytes", MaxPdfSize);
            var maxImageSize = _configuration.GetValue<long>("FileStorage:MaxImageSizeBytes", MaxImageSize);

            // Check if it's a PDF
            var isPdf = await _mimeTypeValidator.IsPdfAsync(file);
            if (isPdf)
            {
                if (file.Length > maxPdfSize)
                {
                    _logger.LogWarning("PDF file size {FileSize} exceeds maximum {MaxSize} for file: {FileName}",
                        file.Length, maxPdfSize, file.FileName);
                    return false;
                }

                // Use Path.GetExtension to handle edge cases like spaces before extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var validPdfExtension = AllowedPdfExtensions.Any(ext => 
                    string.Equals(fileExtension, ext, StringComparison.OrdinalIgnoreCase));
                
                if (!validPdfExtension)
                {
                    _logger.LogWarning("PDF file has invalid extension: {FileName}", file.FileName);
                    return false;
                }

                return true;
            }

            // Check if it's an image
            var isImage = await _mimeTypeValidator.IsImageAsync(file);
            if (isImage)
            {
                if (file.Length > maxImageSize)
                {
                    _logger.LogWarning("Image file size {FileSize} exceeds maximum {MaxSize} for file: {FileName}",
                        file.Length, maxImageSize, file.FileName);
                    return false;
                }

                // Use Path.GetExtension to handle edge cases like spaces before extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var validImageExtension = AllowedImageExtensions.Any(ext => 
                    string.Equals(fileExtension, ext, StringComparison.OrdinalIgnoreCase));
                
                if (!validImageExtension)
                {
                    _logger.LogWarning("Image file has invalid extension: {FileName}", file.FileName);
                    return false;
                }

                return true;
            }

            // File type not recognized
            try
            {
                var ext = Path.GetExtension(file.FileName);
                var detectedMimeType = await _mimeTypeValidator.DetectMimeTypeAsync(file);

                if (string.IsNullOrWhiteSpace(detectedMimeType))
                {
                    using var stream = file.OpenReadStream();
                    var buffer = new byte[12];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var firstBytesHex = bytesRead > 0 ? BitConverter.ToString(buffer, 0, bytesRead) : "EMPTY";

                    _logger.LogWarning(
                        "File type not recognized for file: {FileName} (ext={Ext}, firstBytes={FirstBytes})",
                        file.FileName, ext, firstBytesHex);
                }
                else
                {
                    _logger.LogWarning(
                        "File type not recognized for file: {FileName} (ext={Ext}, detectedMimeType={DetectedMimeType})",
                        file.FileName, ext, detectedMimeType);
                }
            }
            catch
            {
                // Avoid failing validation due to logging/diagnostics issues.
                _logger.LogWarning("File type not recognized for file: {FileName}", file.FileName);
            }

            return false;
        }

        /// <summary>
        /// Validates multiple files for upload with total size check (Phase 7: Multi-file validation)
        /// </summary>
        public async Task<(bool isValid, string? errorMessage)> ValidateMultipleFilesAsync(IEnumerable<IFormFile> files, long? maxTotalSizeBytes = null)
        {
            var fileList = files.ToList();
            
            if (!fileList.Any())
            {
                return (false, "No files provided");
            }

            // Get max total size from config or use provided value
            var maxTotal = maxTotalSizeBytes ?? _configuration.GetValue<long>("FileStorage:MaxTotalSizeBytes", MaxTotalSize);
            
            // Calculate total size
            long totalSize = 0;
            foreach (var file in fileList)
            {
                if (file == null || file.Length == 0)
                {
                    return (false, "One or more files are empty");
                }
                totalSize += file.Length;
            }

            // Check total size
            if (totalSize > maxTotal)
            {
                var totalSizeMB = totalSize / (1024.0 * 1024.0);
                var maxTotalMB = maxTotal / (1024.0 * 1024.0);
                return (false, $"Total file size ({totalSizeMB:F2} MB) exceeds maximum allowed ({maxTotalMB:F2} MB)");
            }

            // Validate each file individually
            var invalidFiles = new List<string>();
            foreach (var file in fileList)
            {
                if (!await ValidateFileAsync(file))
                {
                    invalidFiles.Add(file.FileName);
                }
            }

            if (invalidFiles.Any())
            {
                return (false, $"Invalid files: {string.Join(", ", invalidFiles)}. Files must be PDF or images (JPG, JPEG, PNG, GIF) and each must be under individual size limits.");
            }

            return (true, null);
        }

        /// <summary>
        /// Deletes a file from storage (Phase 6: Soft delete in database)
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                // filePath is actually the FileStorage.Id (GUID)
                if (!Guid.TryParse(filePath, out var fileId))
                {
                    _logger.LogWarning("Invalid file ID format for deletion: {FilePath}", filePath);
                    return false;
                }

                var fileStorage = await _context.FileStorage
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

                if (fileStorage == null)
                {
                    _logger.LogWarning("File not found or already deleted: {FileId}", fileId);
                    return false;
                }

                // Soft delete
                fileStorage.IsDeleted = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("File soft-deleted successfully: {FileId}, EntityType: {EntityType}, EntityId: {EntityId}",
                    fileId, fileStorage.EntityType, fileStorage.EntityId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Gets the maximum file size in bytes (stub returns PDF max size)
        /// </summary>
        public long GetMaxFileSize()
        {
            // Return the configured max PDF size, or default to 10 MB
            var configuredSize = _configuration.GetValue<long>("FileStorage:MaxPdfSizeBytes", MaxPdfSize);
            return configuredSize;
        }

        /// <summary>
        /// Gets the allowed file extensions (returns PDF extensions)
        /// </summary>
        public string[] GetAllowedExtensions()
        {
            // Returns PDF extensions (context-aware extensions handled in ValidateFileAsync)
            return AllowedPdfExtensions;
        }

        /// <summary>
        /// Updates the EntityId of a file (used when entity is created after file upload)
        /// </summary>
        public async Task<bool> UpdateFileEntityIdAsync(Guid fileId, Guid entityId)
        {
            try
            {
                var fileStorage = await _context.FileStorage
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

                if (fileStorage == null)
                {
                    _logger.LogWarning("File not found for EntityId update: {FileId}", fileId);
                    return false;
                }

                fileStorage.EntityId = entityId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("File EntityId updated successfully: {FileId}, New EntityId: {EntityId}", fileId, entityId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update file EntityId: {FileId}, EntityId: {EntityId}", fileId, entityId);
                return false;
            }
        }
    }
}

