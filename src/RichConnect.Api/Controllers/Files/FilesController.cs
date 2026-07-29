using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Utilities.Files;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Files
{
    /// <summary>
    /// Controller for file read operations with streaming, caching, and authorization
    /// Phase 6: Database-only file storage (legacy file system storage removed)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ApiControllerBase
    {
        private readonly IFileReadService _fileReadService;
        private readonly IContentHashHelper _contentHashHelper;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            IFileReadService fileReadService,
            IContentHashHelper contentHashHelper,
            ILogger<FilesController> logger)
        {
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
            _contentHashHelper = contentHashHelper ?? throw new ArgumentNullException(nameof(contentHashHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a file by ID with streaming, ETag support, and authorization checks
        /// Allows anonymous access but still performs authorization checks for file access control
        /// </summary>
        /// <param name="id">The file ID</param>
        /// <returns>File content with appropriate headers, 304 if cached, or 404/403 if not found/unauthorized</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous] // Allow anonymous access - authorization is checked inside the method
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(Guid id)
        {
            try
            {
                // Get file metadata first (lighter query)
                var metadata = await _fileReadService.GetFileMetadataAsync(id);
                if (metadata == null)
                {
                    _logger.LogWarning("File not found: {FileId}", id);
                    return ResourceNotFound("File", id);
                }

                // Authorization check
                // Note: With [AllowAnonymous], User may be unauthenticated, but authorization logic handles this
                var userId = GetCurrentUserId();
                var userRoles = User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var authorized = await _fileReadService.CanUserAccessFileAsync(id, userId == Guid.Empty ? null : userId, userRoles);
                if (!authorized)
                {
                    _logger.LogWarning(
                        "User {UserId} with roles [{Roles}] not authorized to access file {FileId}",
                        userId, string.Join(", ", userRoles), id);
                    
                    return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access denied",
                        Detail = "You do not have permission to access this file",
                        Instance = HttpContext.Request.Path
                    });
                }

                // ETag support - check If-None-Match header
                var etag = _contentHashHelper.ToBase64String(metadata.ContentHash);
                var requestETag = Request.Headers.IfNoneMatch.FirstOrDefault();
                
                if (!string.IsNullOrEmpty(requestETag) && requestETag.Trim('"') == etag)
                {
                    _logger.LogDebug("ETag match, returning 304 Not Modified for file: {FileId}", id);
                    return StatusCode(StatusCodes.Status304NotModified);
                }

                // Get full file with content
                var file = await _fileReadService.GetFileByIdAsync(id);
                if (file == null)
                {
                    // Race condition: file was deleted between metadata and content fetch
                    _logger.LogWarning("File disappeared during fetch: {FileId}", id);
                    return ResourceNotFound("File", id);
                }

                // Determine Content-Disposition
                var dispositionType = DetermineContentDisposition(file.FileCategory, file.MimeType);
                
                // Sanitize filename to prevent header injection and path traversal
                var sanitizedFileName = SanitizeFileName(file.FileName);

                // Set response headers using proper header construction to prevent header injection
                Response.Headers.ContentType = file.MimeType;
                Response.Headers.ContentLength = file.FileSize;
                Response.Headers.ETag = $"\"{etag}\"";
                Response.Headers.CacheControl = "private, max-age=3600"; // Cache for 1 hour
                
                // Use ContentDispositionHeaderValue to safely construct Content-Disposition header
                var contentDisposition = new ContentDispositionHeaderValue(dispositionType)
                {
                    FileName = sanitizedFileName
                };
                Response.Headers.ContentDisposition = contentDisposition.ToString();
                
                Response.Headers.LastModified = file.UploadedAt.ToString("R"); // RFC1123 format

                _logger.LogInformation(
                    "File served successfully. FileId: {FileId}, User: {UserId}, Size: {FileSize} bytes, ETag: {ETag}, FileName: {FileName}",
                    id, userId, file.FileSize, etag, sanitizedFileName);

                // Stream the file content (don't pass filename to File() to avoid double encoding)
                return File(file.Content, file.MimeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FileId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "An error occurred while retrieving the file",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        /// <summary>
        /// Gets all files for an entity, optionally filtered by file category
        /// </summary>
        /// <param name="entityType">The entity type (Challenge, Theme, ResearchField, RDProject)</param>
        /// <param name="entityId">The entity ID</param>
        /// <param name="fileCategory">Optional file category filter (SupportingDocument, Document, etc.). If not provided, returns all files.</param>
        /// <returns>List of file metadata</returns>
        [HttpGet("entity/{entityType}/{entityId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFilesByEntity([FromRoute, StringLength(30, MinimumLength = 1)] string entityType, Guid entityId, [FromQuery] string? fileCategory = null)
        {
            try
            {
                // Validate entity type
                var validEntityTypes = new[] { "Challenge", "Theme", "ResearchField", "RDProject", "Partner" };
                if (!validEntityTypes.Contains(entityType, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid entity type",
                        Detail = $"Entity type must be one of: {string.Join(", ", validEntityTypes)}",
                        Instance = HttpContext.Request.Path
                    });
                }

                var files = await _fileReadService.GetFilesByEntityAsync(entityType, entityId, fileCategory);

                // Check authorization for each file (user must have access to at least view the entity)
                var userId = GetCurrentUserId();
                var userRoles = User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var authorizedFiles = new List<object>();
                foreach (var file in files)
                {
                    var authorized = await _fileReadService.CanUserAccessFileAsync(file.Id, userId == Guid.Empty ? null : userId, userRoles);
                    if (authorized)
                    {
                        authorizedFiles.Add(new
                        {
                            id = file.Id,
                            entityType = file.EntityType,
                            entityId = file.EntityId,
                            fileCategory = file.FileCategory,
                            fileName = file.FileName,
                            mimeType = file.MimeType,
                            fileSize = file.FileSize,
                            contentHash = _contentHashHelper.ToHexString(file.ContentHash),
                            uploadedAt = file.UploadedAt,
                            downloadUrl = $"/api/files/{file.Id}"
                        });
                    }
                }

                _logger.LogInformation(
                    "Retrieved {Count} authorized files for EntityType: {EntityType}, EntityId: {EntityId}, UserId: {UserId}",
                    authorizedFiles.Count, entityType, entityId, userId);

                return SuccessResponse(authorizedFiles, $"Retrieved {authorizedFiles.Count} file(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving files for entity: {EntityType}, {EntityId}", entityType, entityId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "An error occurred while retrieving files",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        /// <summary>
        /// Gets file metadata without downloading content
        /// </summary>
        /// <param name="id">The file ID</param>
        /// <returns>File metadata</returns>
        [HttpGet("{id:guid}/metadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileMetadata(Guid id)
        {
            try
            {
                var metadata = await _fileReadService.GetFileMetadataAsync(id);
                if (metadata == null)
                {
                    _logger.LogWarning("File metadata not found: {FileId}", id);
                    return ResourceNotFound("File", id);
                }

                // Authorization check
                // Note: With [AllowAnonymous], User may be unauthenticated, but authorization logic handles this
                var userId = GetCurrentUserId();
                var userRoles = User?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();

                var authorized = await _fileReadService.CanUserAccessFileAsync(id, userId == Guid.Empty ? null : userId, userRoles);
                if (!authorized)
                {
                    _logger.LogWarning(
                        "User {UserId} with roles [{Roles}] not authorized to access file metadata {FileId}",
                        userId, string.Join(", ", userRoles), id);
                    
                    return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Access denied",
                        Detail = "You do not have permission to access this file",
                        Instance = HttpContext.Request.Path
                    });
                }

                var response = new
                {
                    id = metadata.Id,
                    entityType = metadata.EntityType,
                    entityId = metadata.EntityId,
                    fileCategory = metadata.FileCategory,
                    fileName = metadata.FileName,
                    mimeType = metadata.MimeType,
                    fileSize = metadata.FileSize,
                    contentHash = _contentHashHelper.ToHexString(metadata.ContentHash),
                    uploadedAt = metadata.UploadedAt,
                    etag = _contentHashHelper.ToBase64String(metadata.ContentHash)
                };

                return SuccessResponse(response, "File metadata retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file metadata: {FileId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "An error occurred while retrieving file metadata",
                    Instance = HttpContext.Request.Path
                });
            }
        }

        /// <summary>
        /// Sanitizes a filename to prevent header injection and path traversal attacks
        /// Removes CR/LF characters, quotes, and other dangerous characters
        /// </summary>
        /// <param name="fileName">The original filename</param>
        /// <returns>A sanitized filename safe for use in HTTP headers</returns>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "download";
            }

            // Get just the filename without path (prevents path traversal)
            var name = Path.GetFileName(fileName);
            
            // Remove CR/LF characters that could be used for header injection
            name = name.Replace("\r", "", StringComparison.Ordinal)
                      .Replace("\n", "", StringComparison.Ordinal)
                      .Replace("\t", " ", StringComparison.Ordinal);
            
            // Replace quotes with safe alternative
            name = name.Replace("\"", "'", StringComparison.Ordinal);
            
            // Remove any remaining control characters
            name = new string(name.Where(c => !char.IsControl(c)).ToArray());
            
            // Ensure we have a valid filename
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == "..")
            {
                return "download";
            }
            
            // Limit length to prevent DoS
            const int maxLength = 255;
            if (name.Length > maxLength)
            {
                var extension = Path.GetExtension(name);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(name);
                name = nameWithoutExt.Substring(0, Math.Min(nameWithoutExt.Length, maxLength - extension.Length)) + extension;
            }
            
            return name;
        }

        /// <summary>
        /// Determines the Content-Disposition header based on file category and MIME type
        /// </summary>
        /// <param name="fileCategory">The file category</param>
        /// <param name="mimeType">The MIME type</param>
        /// <returns>"inline" or "attachment"</returns>
        private string DetermineContentDisposition(string fileCategory, string mimeType)
        {
            // Images and logos can be displayed inline safely
            if (fileCategory == "Logo" || fileCategory == "Image")
            {
                return "inline";
            }

            // For PDFs, use attachment by default to prevent XSS via embedded PDF viewers
            // This is a security best practice from the migration plan
            if (mimeType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            {
                return "attachment";
            }

            // Default to attachment for safety
            return "attachment";
        }
    }
}

