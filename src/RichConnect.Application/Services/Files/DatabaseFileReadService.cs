using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Services.Files
{
    /// <summary>
    /// Database-backed file read service with authorization and caching support
    /// Phase 6: Database-only file storage (legacy file system storage removed)
    /// </summary>
    public class DatabaseFileReadService : IFileReadService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseFileReadService> _logger;

        public DatabaseFileReadService(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<DatabaseFileReadService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a file from database storage by its ID
        /// </summary>
        public async Task<FileReadResult?> GetFileByIdAsync(Guid fileId)
        {
            try
            {
                var fileStorage = await _context.FileStorage
                    .Where(f => f.Id == fileId && !f.IsDeleted)
                    .Select(f => new FileReadResult
                    {
                        Id = f.Id,
                        EntityType = f.EntityType,
                        EntityId = f.EntityId,
                        FileCategory = f.FileCategory,
                        FileName = f.FileName,
                        MimeType = f.MimeType,
                        FileSize = f.FileSize,
                        ContentHash = f.ContentHash,
                        Content = f.Content,
                        UploadedAt = f.UploadedAt
                    })
                    .FirstOrDefaultAsync();

                if (fileStorage == null)
                {
                    _logger.LogWarning("File not found or deleted: {FileId}", fileId);
                    return null;
                }

                _logger.LogInformation(
                    "File retrieved successfully. FileId: {FileId}, EntityType: {EntityType}, " +
                    "Size: {FileSize} bytes",
                    fileId, fileStorage.EntityType, fileStorage.FileSize);

                return fileStorage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve file: {FileId}", fileId);
                throw;
            }
        }

        /// <summary>
        /// Gets file metadata without loading content (for quick lookups)
        /// </summary>
        public async Task<FileMetadata?> GetFileMetadataAsync(Guid fileId)
        {
            try
            {
                var metadata = await _context.FileStorage
                    .Where(f => f.Id == fileId && !f.IsDeleted)
                    .Select(f => new FileMetadata
                    {
                        Id = f.Id,
                        EntityType = f.EntityType,
                        EntityId = f.EntityId,
                        FileCategory = f.FileCategory,
                        FileName = f.FileName,
                        MimeType = f.MimeType,
                        FileSize = f.FileSize,
                        ContentHash = f.ContentHash,
                        UploadedAt = f.UploadedAt
                    })
                    .FirstOrDefaultAsync();

                if (metadata == null)
                {
                    _logger.LogWarning("File metadata not found: {FileId}", fileId);
                }

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve file metadata: {FileId}", fileId);
                throw;
            }
        }

        /// <summary>
        /// Checks if a user has permission to access a file
        /// Authorization logic:
        /// - Admins can access all files
        /// - Partners can access their own logos
        /// - Faculty can access challenge documents they submitted or research themes/fields
        /// - Partners can access challenge documents for challenges they're involved in
        /// </summary>
        public async Task<bool> CanUserAccessFileAsync(Guid fileId, Guid? userId, IEnumerable<string> userRoles)
        {
            try
            {
                var roles = userRoles.ToList();

                // Admins can access everything
                if (roles.Contains("Admin") || roles.Contains("admin"))
                {
                    _logger.LogDebug("Admin user authorized to access file: {FileId}", fileId);
                    return true;
                }

                // Get file metadata
                var metadata = await GetFileMetadataAsync(fileId);
                if (metadata == null)
                {
                    _logger.LogWarning("Authorization check failed: file not found: {FileId}", fileId);
                    return false;
                }

                // Public files (if we implement SecurityTag = "Public" in the future)
                // For now, we'll implement entity-specific checks

                switch (metadata.EntityType)
                {
                    case "Partner":
                        // Partners can access their own logos
                        // Check for all possible role name variations: "Partner", "CommunityPartner", "Community Partner"
                        var isPartnerForLogo = roles.Contains("Partner") || 
                                              roles.Contains("CommunityPartner") || 
                                              roles.Contains("Community Partner");
                        
                        if (isPartnerForLogo && userId.HasValue)
                        {
                            var isOwnPartner = await _context.CommunityPartners
                                .AnyAsync(p => p.Id == metadata.EntityId && p.UserId == userId.Value);
                            
                            if (isOwnPartner)
                            {
                                _logger.LogDebug("Partner authorized to access own logo: {FileId}", fileId);
                                return true;
                            }
                        }
                        
                        // All authenticated users can view partner logos (they're public)
                        if (metadata.FileCategory == "Logo")
                        {
                            _logger.LogDebug("User authorized to access public partner logo: {FileId}", fileId);
                            return true;
                        }
                        break;

                    case "Challenge":
                        // Faculty can access challenges they submitted
                        if ((roles.Contains("Faculty") || roles.Contains("Faculty Specialist") || roles.Contains("FacultySpecialist")) && userId.HasValue)
                        {
                            var isOwnChallenge = await _context.Challenges
                                .AnyAsync(c => c.Id == metadata.EntityId && c.SubmittedBy == userId.Value);
                            
                            if (isOwnChallenge)
                            {
                                _logger.LogDebug("Faculty authorized to access own challenge document: {FileId}", fileId);
                                return true;
                            }
                        }

                        // Partners can access challenges they submitted
                        // Check for all possible role name variations: "Partner", "CommunityPartner", "Community Partner"
                        var isPartner = roles.Contains("Partner") || 
                                       roles.Contains("CommunityPartner") || 
                                       roles.Contains("Community Partner");
                        
                        if (isPartner && userId.HasValue)
                        {
                            _logger.LogDebug("Checking partner authorization for file {FileId}, UserId: {UserId}, EntityId: {EntityId}", 
                                fileId, userId.Value, metadata.EntityId);
                            
                            var partner = await _context.CommunityPartners
                                .FirstOrDefaultAsync(p => p.UserId == userId.Value);

                            if (partner != null)
                            {
                                var isOwnChallenge = await _context.Challenges
                                    .AnyAsync(c => c.Id == metadata.EntityId && c.SubmittedBy == userId.Value);
                                
                                _logger.LogDebug("Challenge ownership check - EntityId: {EntityId}, UserId: {UserId}, IsOwnChallenge: {IsOwnChallenge}",
                                    metadata.EntityId, userId.Value, isOwnChallenge);
                                
                                if (isOwnChallenge)
                                {
                                    _logger.LogDebug("Partner authorized to access own challenge document: {FileId}", fileId);
                                    return true;
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Partner profile not found for UserId: {UserId}", userId.Value);
                            }
                        }
                        else if (isPartner && !userId.HasValue)
                        {
                            _logger.LogWarning("Partner role detected but userId is null/empty for file {FileId}", fileId);
                        }
                        break;

                    case "Theme":
                        // Research themes are generally public for viewing
                        if (metadata.FileCategory == "Image")
                        {
                            _logger.LogDebug("User authorized to access public theme image: {FileId}", fileId);
                            return true;
                        }

                        // Faculty can access all theme documents
                        // Role names can vary between identity providers ("Faculty Specialist" vs "FacultySpecialist")
                        if (roles.Contains("Faculty") || roles.Contains("Faculty Specialist") || roles.Contains("FacultySpecialist"))
                        {
                            _logger.LogDebug("Faculty authorized to access theme document: {FileId}", fileId);
                            return true;
                        }
                        break;

                    case "ResearchField":
                        // Research fields are generally public
                        _logger.LogDebug("User authorized to access public research field file: {FileId}", fileId);
                        return true;

                    default:
                        _logger.LogWarning("Unknown entity type for authorization: {EntityType}, FileId: {FileId}", 
                            metadata.EntityType, fileId);
                        return false;
                }

                _logger.LogWarning(
                    "User {UserId} with roles [{Roles}] not authorized to access file {FileId} " +
                    "(EntityType: {EntityType}, EntityId: {EntityId})",
                    userId, string.Join(", ", roles), fileId, metadata.EntityType, metadata.EntityId);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file access authorization: {FileId}", fileId);
                // Fail-closed: deny access on errors
                return false;
            }
        }

        /// <summary>
        /// Gets file ID from FileStorage by EntityType, EntityId, and FileCategory
        /// </summary>
        public async Task<Guid?> GetFileIdByEntityAsync(string entityType, Guid entityId, string fileCategory)
        {
            try
            {
                var fileId = await _context.FileStorage
                    .Where(f => f.EntityType == entityType 
                        && f.EntityId == entityId 
                        && f.FileCategory == fileCategory 
                        && !f.IsDeleted)
                    .Select(f => f.Id)
                    .FirstOrDefaultAsync();

                return fileId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file ID for EntityType: {EntityType}, EntityId: {EntityId}, FileCategory: {FileCategory}", 
                    entityType, entityId, fileCategory);
                throw;
            }
        }

        /// <summary>
        /// Gets all file metadata for an entity, optionally filtered by file category
        /// </summary>
        public async Task<List<FileMetadata>> GetFilesByEntityAsync(string entityType, Guid entityId, string? fileCategory = null)
        {
            try
            {
                var query = _context.FileStorage
                    .Where(f => f.EntityType == entityType 
                        && f.EntityId == entityId 
                        && !f.IsDeleted);

                // Apply file category filter if provided
                if (!string.IsNullOrEmpty(fileCategory))
                {
                    query = query.Where(f => f.FileCategory == fileCategory);
                }

                var files = await query
                    .Select(f => new FileMetadata
                    {
                        Id = f.Id,
                        EntityType = f.EntityType,
                        EntityId = f.EntityId,
                        FileCategory = f.FileCategory,
                        FileName = f.FileName,
                        MimeType = f.MimeType,
                        FileSize = f.FileSize,
                        ContentHash = f.ContentHash,
                        UploadedAt = f.UploadedAt
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} files for EntityType: {EntityType}, EntityId: {EntityId}, FileCategory: {FileCategory}",
                    files.Count, entityType, entityId, fileCategory ?? "All");

                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get files for EntityType: {EntityType}, EntityId: {EntityId}, FileCategory: {FileCategory}", 
                    entityType, entityId, fileCategory ?? "All");
                throw;
            }
        }

        /// <summary>
        /// Batch retrieves file IDs for multiple entities to avoid N+1 queries
        /// </summary>
        public async Task<Dictionary<Guid, Guid?>> GetFileIdsByEntitiesAsync(string entityType, List<Guid> entityIds, string fileCategory)
        {
            try
            {
                if (!entityIds.Any())
                {
                    return new Dictionary<Guid, Guid?>();
                }

                // Fetch all matching files in one query
                var files = await _context.FileStorage
                    .Where(f => f.EntityType == entityType 
                        && entityIds.Contains(f.EntityId)
                        && f.FileCategory == fileCategory 
                        && !f.IsDeleted)
                    .Select(f => new { f.EntityId, f.Id })
                    .ToListAsync();

                // Build result dictionary with null for entities without files
                var result = entityIds.ToDictionary(id => id, id => (Guid?)null);
                foreach (var file in files)
                {
                    result[file.EntityId] = file.Id;
                }

                _logger.LogDebug(
                    "Batch retrieved {FileCount} files for {EntityCount} entities of type {EntityType}, category {FileCategory}",
                    files.Count, entityIds.Count, entityType, fileCategory);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch get file IDs for EntityType: {EntityType}, FileCategory: {FileCategory}", 
                    entityType, fileCategory);
                throw;
            }
        }
    }
}

