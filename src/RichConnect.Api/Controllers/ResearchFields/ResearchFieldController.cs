using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;
using RICHConnect.Backend.Application.Commands.ResearchFields.CreateField;
using RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField;
using RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField;
using RICHConnect.Backend.Application.Commands.ResearchFields.RejectField;
using RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Application.Services.ResearchFields;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Themes
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResearchFieldController : ApiControllerBase
    {
        private readonly IResearchFieldApplicationService _applicationService;
        private readonly ResearchFieldBusinessRulesService _businessRulesService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ResearchFieldController> _logger;

        public ResearchFieldController(
            IResearchFieldApplicationService applicationService,
            ResearchFieldBusinessRulesService businessRulesService,
            IWebHostEnvironment env,
            ILogger<ResearchFieldController> logger)
        {
            _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: api/ResearchField
        /// List all research fields (accessible to all users, including unauthenticated).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var fields = await _applicationService.GetAllActiveAsync();
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// GET: api/ResearchField/pending
        /// List all pending research fields (Admins only).
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var fields = await _applicationService.GetByStatusAsync(ApprovalStatus.Pending);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving pending research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }
        
        /// <summary>
        /// GET: api/ResearchField/all
        /// List all research fields including inactive ones (Admins only).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllIncludingInactive()
        {
            try
            {
                var fields = await _applicationService.GetAllIncludingInactiveAsync();
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving all research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// GET: api/ResearchField/{id}
        /// Get a single research field by ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid research field ID is required.");
                return ValidationProblem();
            }

            try
            {
                var field = await _applicationService.GetByIdAsync(id);
                if (field == null)
                    return ResourceNotFound("ResearchField", id);

                // Apply access control for non-approved/inactive fields
                if (field.Status != ApprovalStatus.Approved || !field.IsActive)
                {
                    var userId = User?.Identity?.IsAuthenticated == true 
                        ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) 
                        : Guid.Empty;

                    if (userId != Guid.Empty)
                    {
                        var canAccess = await _businessRulesService.CanUserAccessFieldAsync(userId, id);
                        if (!canAccess)
                        {
                            // Return 404 to avoid leaking existence of non-accessible fields
                            return ResourceNotFound("ResearchField", id);
                        }
                    }
                    else
                    {
                        // Unauthenticated users can only access approved and active fields
                        return ResourceNotFound("ResearchField", id);
                    }
                }

                return Ok(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        // Faculty specialist submission endpoint has been removed.
        // Research fields can only be created by admins now.

        /// <summary>
        /// GET: api/ResearchField/available
        /// List all research fields where the current facultySpecialist has been invited or is associated with.
        /// </summary>
        [HttpGet("available")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<IActionResult> GetAvailableFields()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var fields = await _applicationService.GetAvailableFieldsForUserAsync(userId);
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving available research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// GET: api/ResearchField/slug/{slug}
        /// Get a single research field by slug.
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug([FromRoute, StringLength(150, MinimumLength = 1)] string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                ModelState.AddModelError(nameof(slug), "A valid research field slug is required.");
                return ValidationProblem();
            }

            try
            {
                var field = await _applicationService.GetBySlugAsync(slug);
                if (field == null)
                    return ResourceNotFound("ResearchField", slug);

                return Ok(field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving research field by slug: {Slug}. TraceId: {TraceId}", slug, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while retrieving the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        // Faculty specialist submission endpoint has been removed.
        // Research fields can only be created by admins now.

        /// <summary>
        /// POST: api/ResearchField
        /// Create a new research field (Admins only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit for research field icons
        public async Task<IActionResult> Create([FromForm] CreateResearchFieldDto dto)
        {
            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Create command for admin creation
                var command = new CreateFieldCommand(
                    name: dto.Name,
                    submittedBy: adminUserId,
                    category: dto.Category,
                    displayOrder: dto.DisplayOrder,
                    isActive: dto.IsActive,
                    isAdminCreated: true);

                // Execute command through application service
                var result = await _applicationService.CreateFieldAsync(command);
                
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Create research field. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating research field. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while creating the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// PUT: api/ResearchField/{id}
        /// Update an existing research field (Admins only).
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit for research field icons
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateResearchFieldDto dto)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid research field ID is required.");
                return ValidationProblem();
            }

            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Create command for update
                var command = new UpdateFieldCommand(
                    fieldId: id,
                    name: dto.Name,
                    updatedBy: adminUserId,
                    category: dto.Category,
                    displayOrder: dto.DisplayOrder,
                    isActive: dto.IsActive);

                // Execute command through application service
                var result = await _applicationService.UpdateFieldAsync(command);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Update research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (KeyNotFoundException)
            {
                return ResourceNotFound("ResearchField", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while updating the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// POST: api/ResearchField/{id}/approve
        /// Admin approves a pending research field.
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid research field ID is required.");
                return ValidationProblem();
            }

            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Create command for approval
                var command = new ApproveFieldCommand(id, adminUserId);

                // Execute command through application service
                var result = await _applicationService.ApproveFieldAsync(command);
                
                return Ok(new { success = result, message = "Research field approved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Approve research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (KeyNotFoundException)
            {
                return ResourceNotFound("ResearchField", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while approving the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// POST: api/ResearchField/{id}/reject
        /// Admin rejects a pending research field.
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectResearchFieldDto dto)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid research field ID is required.");
                return ValidationProblem();
            }

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                ModelState.AddModelError(nameof(dto.RejectionReason), "Rejection reason is required.");
                return ValidationProblem();
            }

            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Create command for rejection
                var command = new RejectFieldCommand(id, adminUserId, dto.RejectionReason);

                // Execute command through application service
                var result = await _applicationService.RejectFieldAsync(command);
                
                return Ok(new { success = result, message = "Research field rejected successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Reject research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (KeyNotFoundException)
            {
                return ResourceNotFound("ResearchField", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while rejecting the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }
        
        /// <summary>
        /// DELETE: api/ResearchField/{id}
        /// Delete a research field (Admins only).
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid research field ID is required.");
                return ValidationProblem();
            }

            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Create command for deletion
                var command = new DeleteFieldCommand(id, adminUserId);

                // Execute command through application service
                var result = await _applicationService.DeleteFieldAsync(command);
                
                return Ok(new { success = result, message = "Research field deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Delete research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (KeyNotFoundException)
            {
                return ResourceNotFound("ResearchField", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting research field for FieldId: {FieldId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while deleting the research field.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// POST: api/ResearchField/bulk
        /// Bulk create research fields (Admins only).
        /// Automatically skips fields with names that already exist.
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateResearchFieldDto dto)
        {
            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var response = new BulkCreateResearchFieldResponse
                {
                    TotalRequested = dto.Fields.Count
                };

                // Get existing field names for deduplication
                var existingFields = await _applicationService.GetAllIncludingInactiveAsync();
                var existingNames = new HashSet<string>(
                    existingFields.Select(f => f.Name.ToLowerInvariant()),
                    StringComparer.OrdinalIgnoreCase
                );

                foreach (var fieldDto in dto.Fields)
                {
                    try
                    {
                        // Check if field with this name already exists
                        if (existingNames.Contains(fieldDto.Name.Trim().ToLowerInvariant()))
                        {
                            response.Skipped++;
                            response.SkippedNames.Add(fieldDto.Name);
                            continue;
                        }

                        // Create command for admin creation
                        var command = new CreateFieldCommand(
                            name: fieldDto.Name,
                            submittedBy: adminUserId,
                            category: fieldDto.Category,
                            displayOrder: fieldDto.DisplayOrder,
                            isActive: fieldDto.IsActive,
                            isAdminCreated: true);

                        // Execute command through application service
                        var result = await _applicationService.CreateFieldAsync(command);
                        
                        response.SuccessfullyCreated++;
                        response.CreatedFields.Add(result);
                        
                        // Add to existing names to prevent duplicates in same batch
                        existingNames.Add(fieldDto.Name.Trim().ToLowerInvariant());
                    }
                    catch (Exception ex)
                    {
                        response.Failed++;
                        response.Errors.Add(new BulkCreateError
                        {
                            Name = fieldDto.Name,
                            Error = ex.Message
                        });
                        
                        _logger.LogError(ex, "Error creating research field {Name} in bulk operation. TraceId: {TraceId}", 
                            fieldDto.Name, HttpContext.TraceIdentifier);
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while bulk creating research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// DELETE: api/ResearchField/delete-all
        /// Delete all research fields that have no dependencies (Admins only).
        /// </summary>
        [HttpDelete("delete-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var fields = (await _applicationService.GetAllIncludingInactiveAsync()).ToList();

                var response = new BulkDeleteResearchFieldResponse
                {
                    TotalRequested = fields.Count
                };

                foreach (var field in fields)
                {
                    try
                    {
                        var command = new DeleteFieldCommand(field.Id, adminUserId);
                        var deleted = await _applicationService.DeleteFieldAsync(command);

                        if (deleted)
                        {
                            response.SuccessfullyDeleted++;
                            response.DeletedNames.Add(field.Name);
                        }
                        else
                        {
                            response.Failed++;
                            response.Errors.Add(new BulkCreateError
                            {
                                Name = field.Name,
                                Error = "Delete operation returned false."
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        response.Failed++;
                        response.Errors.Add(new BulkCreateError
                        {
                            Name = field.Name,
                            Error = ex.Message
                        });
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in delete-all research fields. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return StatusCode(500, new { message = "An error occurred while deleting research fields.", traceId = HttpContext.TraceIdentifier });
            }
        }

    }
}
