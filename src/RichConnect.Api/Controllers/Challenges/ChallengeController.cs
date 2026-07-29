using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Application.Interfaces;
using RICHConnect.Backend.Application.Interfaces.Challenges;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Validators.Challenges;
using RICHConnect.Backend.Application.Services.Challenges;
using RICHConnect.Backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Challenges
{
    /// <summary>
    ///  challenge management endpoints for all user roles
    /// </summary>
    [ApiController]
    [Route("api/challenges")]
    public class ChallengeController : ApiControllerBase
    {
        private readonly ILogger<ChallengeController> _logger;
        private readonly IChallengeApplicationService _challengeService;
        private readonly IChallengeMatchingService _matchingService;
        private readonly IFileUploadService _fileUploadService;
        private readonly CreateChallengeDtoValidator _createValidator;
        private readonly UpdateChallengeDtoValidator _updateValidator;
        private readonly RejectChallengeDtoValidator _rejectValidator;
        private readonly InviteFacultySpecialistsDtoValidator _inviteValidator;
        private readonly ApproveEditRequestDtoValidator _approveEditValidator;
        private readonly RejectEditRequestDtoValidator _rejectEditValidator;
        private readonly ChallengeBusinessRulesService _businessRulesService;

        public ChallengeController(
            ILogger<ChallengeController> logger,
            IChallengeApplicationService challengeService,
            IChallengeMatchingService matchingService,
            IFileUploadService fileUploadService,
            CreateChallengeDtoValidator createValidator,
            UpdateChallengeDtoValidator updateValidator,
            RejectChallengeDtoValidator rejectValidator,
            InviteFacultySpecialistsDtoValidator inviteValidator,
            ApproveEditRequestDtoValidator approveEditValidator,
            RejectEditRequestDtoValidator rejectEditValidator,
            ChallengeBusinessRulesService businessRulesService)
        {
            _logger = logger;
            _challengeService = challengeService;
            _matchingService = matchingService;
            _fileUploadService = fileUploadService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _rejectValidator = rejectValidator;
            _inviteValidator = inviteValidator;
            _approveEditValidator = approveEditValidator;
            _rejectEditValidator = rejectEditValidator;
            _businessRulesService = businessRulesService;
        }

        #region General Endpoints (All Authenticated Users)

        /// <summary>
        /// Get a specific challenge by ID (accessible to all authenticated users)
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            try
            {
                var result = await _challengeService.GetChallengeByIdAsync(id, userId, userRole);
                return SuccessResponse(result);
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Challenge", id);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to GetById for ChallengeId: {ChallengeId}", id);
                return ForbiddenResponse("Access denied");
            }
        }

        /// <summary>
        /// Get challenges by status (role-filtered for security)
        /// - Admin: sees all challenges of any status
        /// - Community Partner: sees only their own challenges regardless of status
        /// - Faculty Specialist: sees only approved challenges
        /// </summary>
        [HttpGet("by-status/{status}")]
        [Authorize]
        public async Task<IActionResult> GetByStatus([FromRoute, EnumDataType(typeof(ChallengeStatus))] ChallengeStatus status)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var allChallenges = await _challengeService.GetChallengesByStatusAsync(status);
                
                // Apply role-based filtering
                List<ChallengeDto> filteredChallenges;
                if (userRole == "Admin")
                {
                    // Admin sees all challenges
                    filteredChallenges = allChallenges;
                }
                else if (userRole == "Community Partner" || userRole == "CommunityPartner")
                {
                    // Community Partners see only their own challenges
                    filteredChallenges = allChallenges.Where(c => c.SubmittedBy == userId).ToList();
                }
                else if (userRole == "Faculty Specialist" || userRole == "FacultySpecialist")
                {
                    // Faculty Specialists see only approved challenges
                    filteredChallenges = status == ChallengeStatus.Approved 
                        ? allChallenges 
                        : new List<ChallengeDto>();
                }
                else
                {
                    // Other roles see nothing
                    filteredChallenges = new List<ChallengeDto>();
                }
                
                return SuccessResponse(filteredChallenges);
            }
            catch (ArgumentException)
            {
                return ErrorResponse<List<ChallengeDto>>("Invalid status provided.");
            }
        }

        #endregion

        #region Community Partner Endpoints

        /// <summary>
        /// Create a new challenge (Community Partners only)
        /// Supports multiple file uploads (images and PDFs)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB limit for multiple files
        public async Task<IActionResult> Create([FromForm] CreateChallengeDto dto, List<IFormFile>? supportingDocuments)
        {
            if (!TryValidate(dto, _createValidator, "create"))
                return ValidationProblem();

            var userId = GetCurrentUserId();

            // Perform async business rules validation
            var businessRulesResult = await _businessRulesService.ValidateChallengeCreationAsync(
                dto.Title, dto.ResearchFieldId, dto.EstimatedCost, userId);
            
            if (!businessRulesResult.IsValid)
            {
                foreach (var error in businessRulesResult.Errors)
                {
                    ModelState.AddModelError("BusinessRules", error);
                }
                return ValidationProblem();
            }

            // Handle file uploads if provided
            // Note: We use a temporary challenge ID here, then update the files' EntityId after challenge creation
            var tempChallengeId = Guid.NewGuid();
            List<string>? uploadedFileIds = null;
            
            if (supportingDocuments != null && supportingDocuments.Any())
            {
                // Validate all files together (checks total size and individual files)
                var validationResult = await _fileUploadService.ValidateMultipleFilesAsync(supportingDocuments);
                if (!validationResult.isValid)
                {
                    ModelState.AddModelError("supportingDocuments", validationResult.errorMessage ?? "File validation failed");
                    return ValidationProblem();
                }

                try
                {
                    uploadedFileIds = await _fileUploadService.UploadMultipleSupportingDocumentsAsync(
                        supportingDocuments, tempChallengeId.ToString(), userId);
                    
                    // Store the first file ID in the DTO for backwards compatibility
                    if (uploadedFileIds.Any())
                    {
                        dto.SupportingDocumentUrl = uploadedFileIds.First();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload supporting documents for challenge submission");
                    ModelState.AddModelError("supportingDocuments", ex.Message.Contains(":")  ? ex.Message : "Failed to upload files. Please try again.");
                    return ValidationProblem();
                }
            }

            try
            {
                var result = await _challengeService.CreateChallengeAsync(dto, userId);
                
                // Update files' EntityId to match the actual challenge ID if files were uploaded
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
                                _logger.LogError(ex, "Failed to update file EntityId for challenge {ChallengeId}, file {FileId}", result.Id, fileId);
                                // Don't fail the request if file update fails - the file is already uploaded
                            }
                        }
                    }
                }
                
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                // Clean up orphaned files if challenge creation failed
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        try
                        {
                            await _fileUploadService.DeleteFileAsync(uploadedFileId);
                            _logger.LogInformation("Cleaned up orphaned file {FileId} after challenge creation failure", uploadedFileId);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Failed to clean up orphaned file {FileId}", uploadedFileId);
                        }
                    }
                }
                _logger.LogWarning(ex, "Invalid argument in Create challenge: {Message}", ex.Message);
                return ErrorResponse<ChallengeDto>("Invalid request data");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Clean up orphaned files if challenge creation failed
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        try
                        {
                            await _fileUploadService.DeleteFileAsync(uploadedFileId);
                            _logger.LogInformation("Cleaned up orphaned file {FileId} after database error", uploadedFileId);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Failed to clean up orphaned file {FileId}", uploadedFileId);
                        }
                    }
                }
                
                // Log the inner exception for debugging
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Database error while creating challenge: {InnerException}", innerException);
                
                // SECURITY FIX: Don't expose database error details
                return ErrorResponse<ChallengeDto>("Failed to save challenge. Please try again or contact support.");
            }
            catch (Exception ex)
            {
                // Clean up orphaned files if challenge creation failed
                if (uploadedFileIds != null && uploadedFileIds.Any())
                {
                    foreach (var uploadedFileId in uploadedFileIds)
                    {
                        try
                        {
                            await _fileUploadService.DeleteFileAsync(uploadedFileId);
                            _logger.LogInformation("Cleaned up orphaned file {FileId} after unexpected error", uploadedFileId);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Failed to clean up orphaned file {FileId}", uploadedFileId);
                        }
                    }
                }
                
                // Log the full exception with inner exception details
                var innerException = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Unexpected error while creating challenge: {InnerException}", innerException);
                
                // SECURITY FIX: Don't expose internal error details
                return ErrorResponse<ChallengeDto>("An unexpected error occurred. Please try again or contact support.");
            }
        }

        /// <summary>
        /// Get all challenges submitted by the current user (Community Partners only)
        /// </summary>
        [HttpGet("partner/my")]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        public async Task<IActionResult> GetMyChallenges()
        {
            var userId = GetCurrentUserId();
            var result = await _challengeService.GetUserChallengesAsync(userId);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Update an existing challenge (Admin only)
        /// Supports multiple file uploads (images and PDFs)
        /// </summary>
        [HttpPut("admin/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB limit for multiple files
        public async Task<IActionResult> AdminUpdateChallenge(Guid id, [FromForm] UpdateChallengeDto dto, List<IFormFile>? supportingDocuments)
        {
            if (!TryValidate(dto, _updateValidator, "update"))
                return ValidationProblem();

            var adminId = GetCurrentUserId();

            // Perform async business rules validation
            var businessRulesResult = await _businessRulesService.ValidateChallengeUpdateAsync(
                id, dto.Title, dto.ResearchFieldId, dto.EstimatedCost, adminId, isAdmin: true);
            
            if (!businessRulesResult.IsValid)
            {
                foreach (var error in businessRulesResult.Errors)
                {
                    ModelState.AddModelError("BusinessRules", error);
                }
                return ValidationProblem();
            }

            var userId = GetCurrentUserId();
            
            // Handle file uploads if provided
            if (supportingDocuments != null && supportingDocuments.Any())
            {
                var validationResult = await _fileUploadService.ValidateMultipleFilesAsync(supportingDocuments);
                if (!validationResult.isValid)
                {
                    ModelState.AddModelError("supportingDocuments", validationResult.errorMessage ?? "File validation failed");
                    return ValidationProblem();
                }

                try
                {
                    var uploadedFileIds = await _fileUploadService.UploadMultipleSupportingDocumentsAsync(
                        supportingDocuments, id.ToString(), userId);
                    
                    // Store the first file ID in the DTO for backwards compatibility
                    if (uploadedFileIds.Any())
                    {
                        dto.SupportingDocumentUrl = uploadedFileIds.First();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload supporting documents for challenge update");
                    ModelState.AddModelError("supportingDocuments", ex.Message.Contains(":") ? ex.Message : "Failed to upload files. Please try again.");
                    return ValidationProblem();
                }
            }

            try
            {
                var result = await _challengeService.UpdateChallengeAsync(id, dto, adminId);
                return SuccessResponse(result, "Challenge updated successfully by admin.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in AdminUpdateChallenge for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in AdminUpdateChallenge for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeDto>("Invalid operation");
            }
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Get all pending challenges (Admin only)
        /// </summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _challengeService.GetChallengesByStatusWithDetailsAsync(ChallengeStatus.Pending);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get all challenges regardless of status (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var pending = await _challengeService.GetChallengesByStatusAsync(ChallengeStatus.Pending);
            var approved = await _challengeService.GetChallengesByStatusAsync(ChallengeStatus.Approved);
            var rejected = await _challengeService.GetChallengesByStatusAsync(ChallengeStatus.Rejected);
            var matched = await _challengeService.GetChallengesByStatusAsync(ChallengeStatus.Matched);

            var allChallenges = pending.Concat(approved).Concat(rejected).Concat(matched).ToList();
            return SuccessResponse(allChallenges);
        }

        /// <summary>
        /// Get all approved challenges (Admin only)
        /// </summary>
        [HttpGet("admin/approved")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApproved()
        {
            var result = await _challengeService.GetApprovedChallengesForMatchingAsync();
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get all rejected challenges (Admin only)
        /// </summary>
        [HttpGet("admin/rejected")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRejected()
        {
            var result = await _challengeService.GetChallengesByStatusWithDetailsAsync(ChallengeStatus.Rejected);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get all matched challenges (Admin only)
        /// </summary>
        [HttpGet("admin/matched")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMatched()
        {
            var result = await _challengeService.GetChallengesByStatusWithDetailsAsync(ChallengeStatus.Matched);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Approve a pending challenge (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var result = await _challengeService.ApproveChallengeAsync(id, adminId);
                return SuccessResponse(result, "Challenge approved successfully.");
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Challenge", id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Approve for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeDto>("Invalid operation");
            }
        }

        /// <summary>
        /// Reject a pending challenge (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectChallengeDto dto)
        {
            if (!TryValidate(dto, _rejectValidator))
                return ValidationProblem();

            var adminId = GetCurrentUserId();

            try
            {
                var result = await _challengeService.RejectChallengeAsync(id, dto, adminId);
                return SuccessResponse(result, "Challenge rejected successfully.");
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Challenge", id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Reject for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeDto>("Invalid operation");
            }
        }

        /// <summary>
        /// Invite faculty specialists to a challenge (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/invite-faculty-specialists")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InviteFacultySpecialists(Guid id, [FromBody] InviteFacultySpecialistsDto dto)
        {
            if (!TryValidate(dto, _inviteValidator))
                return ValidationProblem();

            try
            {
                var result = await _matchingService.InviteFacultySpecialistsAsync(id, dto.FacultySpecialistIds);
                return SuccessResponse(result);
            }
            catch (ArgumentException)
            {
                return ErrorResponse<List<MatchInviteDto>>("Invalid challenge or facultySpecialist IDs provided.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in InviteFacultySpecialists for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<List<MatchInviteDto>>("Invalid operation");
            }
        }

        /// <summary>
        /// Get invites for a specific challenge (Admin only)
        /// </summary>
        [HttpGet("{id:guid}/invites")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInvitesForChallenge(Guid id)
        {
            try
            {
                var result = await _matchingService.GetInvitesForChallengeAsync(id);
                return SuccessResponse(result);
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Challenge", id);
            }
        }

        /// <summary>
        /// Finalize matching for a challenge (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/finalize-match")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinalizeMatch(Guid id)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var result = await _challengeService.FinalizeMatchingAsync(id, adminId);
                return SuccessResponse(result);
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Challenge", id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in FinalizeMatch for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<MatchFinalizeDto>("Invalid operation");
            }
        }

        #endregion

        #region Faculty Specialist Endpoints

        /// <summary>
        /// Get all invites for the current faculty specialist (Faculty Specialist only)
        /// </summary>
        [HttpGet("faculty-specialist/invites/my")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<IActionResult> GetMyInvites()
        {
            var facultySpecialistId = GetCurrentUserId();
            var result = await _matchingService.GetFacultySpecialistInvitesAsync(facultySpecialistId);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Respond to a challenge invite (Faculty Specialist only)
        /// </summary>
        [HttpPost("faculty-specialist/invites/{inviteId:guid}/respond")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<IActionResult> RespondToInvite(Guid inviteId, [FromBody] RespondToInviteDto dto)
        {
            if (dto.Decision != InviteStatus.Accepted && dto.Decision != InviteStatus.Rejected)
            {
                return ErrorResponse<MatchResponseDto>("Decision must be Accepted or Rejected.");
            }

            var facultySpecialistId = GetCurrentUserId();

            try
            {
                var result = await _matchingService.RespondToInviteAsync(inviteId, facultySpecialistId, dto.Decision);
                return SuccessResponse(result);
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Invite", inviteId);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in RespondToInvite for InviteId: {InviteId}", inviteId);
                return ForbiddenResponse("Access denied");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in RespondToInvite for InviteId: {InviteId}", inviteId);
                return ErrorResponse<MatchResponseDto>("Invalid operation");
            }
        }

        /// <summary>
        /// Get all challenges the faculty specialist is participating in (Faculty Specialist only)
        /// </summary>
        [HttpGet("faculty-specialist/participating")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<IActionResult> GetMyParticipatingChallenges()
        {
            var facultySpecialistId = GetCurrentUserId();
            var result = await _matchingService.GetFacultySpecialistParticipatingAsync(facultySpecialistId);
            return SuccessResponse(result);
        }

        #endregion

        #region Challenge Edit Requests

        /// <summary>
        /// Request an edit for a submitted challenge (Community Partners only)
        /// </summary>
        [HttpPost("{id:guid}/request-edit")]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        public async Task<IActionResult> RequestEdit(Guid id, [FromBody] RequestChallengeEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = GetCurrentUserId();
            
            try
            {
                var result = await _challengeService.RequestChallengeEditAsync(id, dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.ChallengeId }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in RequestEdit for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in RequestEdit for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid operation");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in RequestEdit for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeEditRequestDto>("Access denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RequestEdit for ChallengeId: {ChallengeId}, UserId: {UserId}", id, userId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get the status of an edit request for a challenge (Community Partners only)
        /// </summary>
        [HttpGet("{id:guid}/edit-request-status")]
        [Authorize(Roles = "Community Partner,CommunityPartner")]
        public async Task<IActionResult> GetEditRequestStatus(Guid id)
        {
            var userId = GetCurrentUserId();
            
            try
            {
                var result = await _challengeService.GetEditRequestStatusAsync(id, userId);
                if (result == null)
                {
                    return Ok(new { data = (ChallengeEditRequestDto?)null });
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in GetEditRequestStatus for ChallengeId: {ChallengeId}", id);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in GetEditRequestStatus for ChallengeId: {ChallengeId}", id);
                return ForbiddenResponse("Access denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetEditRequestStatus for ChallengeId: {ChallengeId}, UserId: {UserId}", id, userId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        /// <summary>
        /// Approve a challenge edit request (Admin only)
        /// </summary>
        [HttpPost("edit-requests/{editRequestId:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveEditRequest(Guid editRequestId, [FromBody] ApproveEditRequestDto dto)
        {
            if (!TryValidate(dto, _approveEditValidator))
                return ValidationProblem();

            var adminId = GetCurrentUserId();

            try
            {
                var result = await _challengeService.ApproveEditRequestAsync(editRequestId, dto, adminId);
                return SuccessResponse(result, "Edit request approved successfully.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in ApproveEditRequest for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in ApproveEditRequest for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ApproveEditRequest for EditRequestId: {EditRequestId}, AdminId: {AdminId}", editRequestId, adminId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        /// <summary>
        /// Reject a challenge edit request (Admin only)
        /// </summary>
        [HttpPost("edit-requests/{editRequestId:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectEditRequest(Guid editRequestId, [FromBody] RejectEditRequestDto dto)
        {
            if (!TryValidate(dto, _rejectEditValidator))
                return ValidationProblem();

            var adminId = GetCurrentUserId();

            try
            {
                var result = await _challengeService.RejectEditRequestAsync(editRequestId, dto, adminId);
                return SuccessResponse(result, "Edit request rejected successfully.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in RejectEditRequest for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in RejectEditRequest for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in RejectEditRequest for EditRequestId: {EditRequestId}, AdminId: {AdminId}", editRequestId, adminId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get challenge edit request details (Admin only)
        /// </summary>
        [HttpGet("edit-requests/{editRequestId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEditRequestDetails(Guid editRequestId)
        {
            try
            {
                var result = await _challengeService.GetEditRequestDetailsAsync(editRequestId);
                if (result == null)
                {
                    return ResourceNotFound("Edit Request", editRequestId);
                }
                return SuccessResponse(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in GetEditRequestDetails for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetEditRequestDetails for EditRequestId: {EditRequestId}", editRequestId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        /// <summary>
        /// Get challenge edit request details by challenge ID (Admin only)
        /// </summary>
        [HttpGet("{challengeId:guid}/edit-request")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEditRequestByChallengeId(Guid challengeId)
        {
            try
            {
                _logger.LogInformation("Getting edit request for challenge ID: {ChallengeId}", challengeId);
                var result = await _challengeService.GetEditRequestByChallengeIdAsync(challengeId);
                if (result == null)
                {
                    _logger.LogWarning("No edit request found for challenge ID: {ChallengeId}", challengeId);
                    return ResourceNotFound("Edit Request", challengeId);
                }
                _logger.LogInformation("Found edit request for challenge ID: {ChallengeId}, Status: {Status}", challengeId, result.Status);
                return SuccessResponse(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in GetEditRequestByChallengeId for ChallengeId: {ChallengeId}", challengeId);
                return ErrorResponse<ChallengeEditRequestDto>("Invalid request data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetEditRequestByChallengeId for ChallengeId: {ChallengeId}", challengeId);
                return ErrorResponse<ChallengeEditRequestDto>("An unexpected error occurred while processing your request");
            }
        }

        #endregion
    }
}
