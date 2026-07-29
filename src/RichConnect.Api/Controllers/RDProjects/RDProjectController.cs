using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.RDProject;
using RICHConnect.Backend.Application.Interfaces.RDProjects;
using RICHConnect.Backend.Application.Interfaces.Files;

namespace RICHConnect.Backend.Api.Controllers.RDProjects
{
    [ApiController]
    [Route("api/rd-projects")]
    public class RDProjectController : ApiControllerBase
    {
        private readonly ILogger<RDProjectController> _logger;
        private readonly IRDProjectApplicationService _rdProjectService;
        private readonly IFileUploadService _fileUploadService;

        public RDProjectController(
            ILogger<RDProjectController> logger,
            IRDProjectApplicationService rdProjectService,
            IFileUploadService fileUploadService)
        {
            _logger = logger;
            _rdProjectService = rdProjectService;
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Create a new R&D project (Community Partners only)
        /// Supports multiple file uploads (documents and images)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB limit for multiple files
        public async Task<IActionResult> Create([FromForm] CreateRDProjectDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();

            var userId = GetCurrentUserId();
            
            // Handle file uploads if provided
            var tempRDProjectId = Guid.NewGuid();
            List<string>? uploadedFileIds = null;
            
            if (dto.SupportingDocuments != null && dto.SupportingDocuments.Any())
            {
                // Validate all files together (checks total size and individual files)
                var validationResult = await _fileUploadService.ValidateMultipleFilesAsync(dto.SupportingDocuments);
                if (!validationResult.isValid)
                {
                    ModelState.AddModelError("supportingDocuments", validationResult.errorMessage ?? "File validation failed");
                    return ValidationProblem();
                }

                try
                {
                    uploadedFileIds = await _fileUploadService.UploadMultipleFilesAsync(
                        dto.SupportingDocuments, "RDProject", tempRDProjectId, "SupportingDocument", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload supporting documents for R&D project submission");
                    ModelState.AddModelError("supportingDocuments", ex.Message.Contains(":") ? ex.Message : "Failed to upload files. Please try again.");
                    return ValidationProblem();
                }
            }

            try
            {
                var result = await _rdProjectService.CreateRDProjectAsync(dto, userId);
                
                // Update files' EntityId to match the actual R&D project ID if files were uploaded
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        if (Guid.TryParse(uploadedFileId, out var fileId))
                        {
                            try
                            {
                                await _fileUploadService.UpdateFileEntityIdAsync(fileId, result.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to update file EntityId for R&D project {ProjectId}, file {FileId}", result.Id, fileId);
                                // Don't fail the request if file update fails - the file is already uploaded
                            }
                        }
                    }
                }
                
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                // Clean up orphaned files if R&D project creation failed
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        try
                        {
                            await _fileUploadService.DeleteFileAsync(uploadedFileId);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Failed to clean up orphaned file {FileId}", uploadedFileId);
                        }
                    }
                }
                
                _logger.LogWarning(ex, "Invalid argument in Create R&D project for UserId: {UserId}", userId);
                return ErrorResponse<RDProjectDto>("Invalid request data");
            }
            catch (Exception ex)
            {
                // Clean up orphaned files on unexpected errors
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        try
                        {
                            await _fileUploadService.DeleteFileAsync(uploadedFileId);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Failed to clean up orphaned file {FileId}", uploadedFileId);
                        }
                    }
                }
                
                _logger.LogError(ex, "Unexpected error while creating R&D project for UserId: {UserId}", userId);
                return ErrorResponse<RDProjectDto>("An unexpected error occurred. Please try again or contact support.");
            }
        }

        /// <summary>
        /// Get a specific R&D project by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _rdProjectService.GetRDProjectByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound($"R&D project with ID {id} not found.");
                }

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving R&D project with ID: {ProjectId}", id);
                return ErrorResponse<RDProjectDto>("An error occurred while retrieving the R&D project.");
            }
        }

        /// <summary>
        /// Get all R&D projects submitted by the current user (Community Partners only)
        /// </summary>
        [HttpGet("partner/my")]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        public async Task<IActionResult> GetMyRDProjects()
        {
            var userId = GetCurrentUserId();
            var result = await _rdProjectService.GetUserRDProjectsAsync(userId);
            return SuccessResponse(result);
        }

        #region Admin Endpoints

        /// <summary>
        /// Get all pending R&D projects (Admin only)
        /// </summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Pending);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get all R&D projects regardless of status (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var pending = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Pending);
            var approved = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Approved);
            var rejected = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Rejected);
            var matched = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Matched);

            var allProjects = pending.Concat(approved).Concat(rejected).Concat(matched).ToList();
            return SuccessResponse(allProjects);
        }

        /// <summary>
        /// Get all approved R&D projects (Admin only)
        /// </summary>
        [HttpGet("admin/approved")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApproved()
        {
            var result = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Approved);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get all rejected R&D projects (Admin only)
        /// </summary>
        [HttpGet("admin/rejected")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRejected()
        {
            var result = await _rdProjectService.GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus.Rejected);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Approve a pending R&D project (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var result = await _rdProjectService.ApproveRDProjectAsync(id, adminId);
                return SuccessResponse(result, "R&D project approved successfully.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in Approve R&D project for ProjectId: {ProjectId}", id);
                return ErrorResponse<RDProjectDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Approve R&D project for ProjectId: {ProjectId}", id);
                return ErrorResponse<RDProjectDto>("Invalid operation");
            }
        }

        /// <summary>
        /// Reject a pending R&D project (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string rejectionReason)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var result = await _rdProjectService.RejectRDProjectAsync(id, adminId, rejectionReason);
                return SuccessResponse(result, "R&D project rejected.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in Reject R&D project for ProjectId: {ProjectId}", id);
                return ErrorResponse<RDProjectDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Reject R&D project for ProjectId: {ProjectId}", id);
                return ErrorResponse<RDProjectDto>("Invalid operation");
            }
        }

        #endregion
    }
}
