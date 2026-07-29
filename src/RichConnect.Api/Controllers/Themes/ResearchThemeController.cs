// File: Controllers/ThemeController.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Application.Commands.Themes.SubmitTheme;
using RICHConnect.Backend.Application.Commands.Themes.ApproveTheme;
using RICHConnect.Backend.Application.Commands.Themes.RejectTheme;
using RICHConnect.Backend.Application.Commands.Themes.UpdateTheme;
using RICHConnect.Backend.Application.Commands.Themes.DeleteTheme;
using RICHConnect.Backend.Application.Commands.Themes.PublishTheme;
using RICHConnect.Backend.Application.Commands.Themes.UnpublishTheme;
using RICHConnect.Backend.Application.Queries.Themes.GetThemeById;
using RICHConnect.Backend.Application.Queries.Themes.GetThemeBySlug;
using RICHConnect.Backend.Application.Queries.Themes.GetThemesByStatus;
using RICHConnect.Backend.Application.Queries.Themes.GetUserThemes;
using RICHConnect.Backend.Application.Queries.Themes.GetAllThemes;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Application.Validators.Themes;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Themes
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResearchThemeController : ApiControllerBase
    {
        private readonly IThemeApplicationService _themeApplicationService;
        private readonly IThemeBusinessRulesService _businessRulesService;
        private readonly ResearchThemeDtoValidator _validator;
        private readonly FacultySpecialistResearchThemeSubmissionDtoValidator _facultySpecialistValidator;
        private readonly AdminResearchThemeCreationDtoValidator _adminValidator;
        private readonly AdminResearchThemeUpdateDtoValidator _adminUpdateValidator;
        private readonly IWebHostEnvironment _env;

        public ResearchThemeController(
            IThemeApplicationService themeApplicationService,
            IThemeBusinessRulesService businessRulesService,
            ResearchThemeDtoValidator validator,
            FacultySpecialistResearchThemeSubmissionDtoValidator facultySpecialistValidator,
            AdminResearchThemeCreationDtoValidator adminValidator,
            AdminResearchThemeUpdateDtoValidator adminUpdateValidator,
            IWebHostEnvironment env)
        {
            _themeApplicationService = themeApplicationService;
            _businessRulesService = businessRulesService;
            _validator = validator;
            _facultySpecialistValidator = facultySpecialistValidator;
            _adminValidator = adminValidator;
            _adminUpdateValidator = adminUpdateValidator;
            _env = env;
        }

        /// <summary>
        /// POST: api/Theme
        /// Body: multipart/form-data with fields from FacultySpecialistThemeSubmissionDto including optional document files
        /// Only Faculty Specialists can submit a new theme proposal.
        /// Supports multiple file uploads.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB limit for multiple theme documents
        public async Task<IActionResult> Create([FromForm] FacultySpecialistThemeSubmissionDto dto)
        {
            try
            {
                // Validate the request
                if (!TryValidate(dto, _facultySpecialistValidator, "create"))
                {
                    return ValidationProblem();
                }

                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Merge single file with multiple files for backwards compatibility
                var documents = new List<IFormFile>();
                if (dto.Documents != null && dto.Documents.Any())
                {
                    documents.AddRange(dto.Documents);
                }
                else if (dto.Document != null)
                {
                    documents.Add(dto.Document);
                }

                // Create command
                var command = new SubmitThemeCommand
                {
                    Title = dto.Title.Trim(),
                    SubmittedBy = userId,
                    Description = dto.Description?.Trim(),
                    ExpectedOutcomes = dto.ExpectedOutcomes?.Trim(),
                    EstimatedFunding = dto.EstimatedFunding,
                    ResearchFieldId = dto.ResearchFieldId,
                    Documents = documents.Any() ? documents : null,
                    Document = documents.FirstOrDefault(), // For backwards compatibility
                    IsAdminCreated = false
                };

                // Submit theme using application service
                var result = await _themeApplicationService.SubmitThemeAsync(command);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.Error.WriteLine($"Error creating theme: {ex.Message}");
                return ErrorResponse<string>("An error occurred while creating the theme.");
            }
        }

        /// <summary>
        /// GET: api/Theme/pending
        /// List all themes with Status = Pending (Admins only).
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var query = new GetThemesByStatusQuery
                {
                    Status = ApprovalStatus.Pending
                };

                var themes = await _themeApplicationService.GetThemesByStatusAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting pending themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving pending themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/approved
        /// List all approved and published themes (accessible to all users, including unauthenticated).
        /// </summary>
        [HttpGet("approved")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApproved()
        {
            try
            {
                var query = new GetThemesByStatusQuery
                {
                    Status = ApprovalStatus.Approved,
                    OnlyPublished = true // Only return published themes for public access
                };

                var themes = await _themeApplicationService.GetThemesByStatusAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting approved themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving approved themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/approved/all
        /// List all approved themes (published and unpublished) for admin dashboard (Admins only).
        /// </summary>
        [HttpGet("approved/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovedAll()
        {
            try
            {
                var query = new GetThemesByStatusQuery
                {
                    Status = ApprovalStatus.Approved,
                    OnlyPublished = false // Return all approved themes (published and unpublished) for admin
                };

                var themes = await _themeApplicationService.GetThemesByStatusAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting all approved themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving approved themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/rejected
        /// List all rejected themes (Admins only).
        /// </summary>
        [HttpGet("rejected")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRejected()
        {
            try
            {
                var query = new GetThemesByStatusQuery
                {
                    Status = ApprovalStatus.Rejected
                };

                var themes = await _themeApplicationService.GetThemesByStatusAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting rejected themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving rejected themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/all
        /// List all themes regardless of status (Admins only).
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var query = new GetAllThemesQuery();

                var themes = await _themeApplicationService.GetAllThemesAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting all themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/my
        /// List all themes submitted by the current faculty specialist.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<IActionResult> GetMyThemes()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var query = new GetUserThemesQuery
                {
                    UserId = userId
                };

                var themes = await _themeApplicationService.GetUserThemesAsync(query);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting user themes: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving your themes.");
            }
        }

        /// <summary>
        /// GET: api/Theme/slug/{slug}
        /// Get a single theme by slug (accessible to all users, including unauthenticated).
        /// Only approved themes are publicly accessible. Pending/rejected themes require admin access.
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug([FromRoute, StringLength(150, MinimumLength = 1)] string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    ModelState.AddModelError(nameof(slug), "A valid theme slug is required.");
                    return ValidationProblem();
                }

                var query = new GetThemeBySlugQuery
                {
                    Slug = slug
                };

                var theme = await _themeApplicationService.GetThemeBySlugAsync(query);
                
                if (theme == null)
                    return ResourceNotFound("Theme", slug);

                // SECURITY: Only approved themes are publicly accessible
                // Admins can view any theme; others can only view approved themes
                var isAdmin = User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
                
                if (theme.Status != ApprovalStatus.Approved && !isAdmin)
                {
                    // Return 404 to avoid leaking existence of pending/rejected themes
                    return ResourceNotFound("Theme", slug);
                }

                return Ok(theme);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting theme by slug: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving the theme.");
            }
        }

        /// <summary>
        /// GET: api/Theme/{id}
        /// Get a single theme by ID (accessible to all users, including unauthenticated).
        /// Only approved themes are publicly accessible. Pending/rejected themes require admin access.
        /// </summary>
        [HttpGet("{id:guid}", Order = 100)]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                    return ValidationProblem();
                }

                var query = new GetThemeByIdQuery
                {
                    ThemeId = id
                };

                var theme = await _themeApplicationService.GetThemeByIdAsync(query);
                
                if (theme == null)
                    return ResourceNotFound("Theme", id);

                // Use business rules for access control
                var userId = User?.Identity?.IsAuthenticated == true 
                    ? Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) 
                    : Guid.Empty;

                if (userId != Guid.Empty)
                {
                    var accessValidation = await _businessRulesService.CanUserAccessThemeAsync(id, userId);
                    if (!accessValidation.IsValid)
                    {
                        // Return 404 to avoid leaking existence of non-accessible themes
                        return ResourceNotFound("Theme", id);
                    }
                }
                else if (theme.Status != ApprovalStatus.Approved)
                {
                    // Unauthenticated users can only access approved themes
                    return ResourceNotFound("Theme", id);
                }

                return Ok(theme);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting theme by ID: {ex.Message}");
                return ErrorResponse<string>("An error occurred while retrieving the theme.");
            }
        }

        /// <summary>
        /// POST: api/Theme/{id}/approve
        /// Admin approves a pending theme.
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                    return ValidationProblem();
                }

                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var command = new ApproveThemeCommand
                {
                    ThemeId = id,
                    ApprovedBy = adminUserId
                };

                var result = await _themeApplicationService.ApproveThemeAsync(command);
                
                return Ok(new { message = "Theme approved successfully.", theme = result });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error approving theme: {ex.Message}");
                return ErrorResponse<string>("An error occurred while approving the theme.");
            }
        }

        /// <summary>
        /// POST: api/Theme/{id}/reject
        /// Admin rejects a pending theme.
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectThemeDto dto)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                    return ValidationProblem();
                }

                if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                {
                    ModelState.AddModelError(nameof(dto.RejectionReason), "Rejection reason is required.");
                    return ValidationProblem();
                }

                var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var command = new RejectThemeCommand
                {
                    ThemeId = id,
                    RejectedBy = adminUserId,
                    RejectionReason = dto.RejectionReason.Trim()
                };

                var result = await _themeApplicationService.RejectThemeAsync(command);
                
                return Ok(new { message = "Theme rejected successfully.", theme = result });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error rejecting theme: {ex.Message}");
                return ErrorResponse<string>("An error occurred while rejecting the theme.");
            }
        }

    /// <summary>
    /// POST: api/Theme/admin
    /// Body: multipart/form-data with fields from AdminThemeCreationDto including optional image files
    /// Allows admins to directly add a new theme (automatically approved).
    /// Supports multiple file uploads.
    /// </summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(25 * 1024 * 1024)] // 25 MB limit for multiple theme images
    public async Task<IActionResult> AdminCreate([FromForm] AdminThemeCreationDto dto)
    {
        try
        {
            // Validate the request
            if (!TryValidate(dto, _adminValidator, "create"))
            {
                return ValidationProblem();
            }

            var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Merge single file with multiple files for backwards compatibility
            var images = new List<IFormFile>();
            if (dto.Images != null && dto.Images.Any())
            {
                images.AddRange(dto.Images);
            }
            else if (dto.Image != null)
            {
                images.Add(dto.Image);
            }

            // Create command for admin-created theme
            var command = new SubmitThemeCommand
            {
                Title = dto.Title.Trim(),
                SubmittedBy = adminUserId,
                Description = dto.Description?.Trim(),
                ExpectedOutcomes = dto.ExpectedOutcomes?.Trim(),
                EstimatedFunding = dto.EstimatedFunding,
                ResearchFieldId = dto.ResearchFieldId,
                Images = images.Any() ? images : null,
                Image = images.FirstOrDefault(), // For backwards compatibility
                IsAdminCreated = true
            };

            // Submit theme using application service (will be auto-approved)
            var result = await _themeApplicationService.SubmitThemeAsync(command);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error creating admin theme: {ex.Message}");
            return ErrorResponse<string>("An error occurred while creating the theme.");
        }
    }

    /// <summary>
    /// PUT: api/Theme/{id}
    /// Body: multipart/form-data with fields from AdminThemeUpdateDto including an optional image file
    /// Only Admins can update themes.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit for theme images
    public async Task<IActionResult> Update(Guid id, [FromForm] AdminThemeUpdateDto dto)
    {
        try
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                return ValidationProblem();
            }

            // Validate the request
            if (!TryValidate(dto, _adminUpdateValidator))
            {
                return ValidationProblem();
            }

            var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var command = new UpdateThemeCommand
            {
                ThemeId = id,
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                ExpectedOutcomes = dto.ExpectedOutcomes?.Trim(),
                EstimatedFunding = dto.EstimatedFunding,
                ResearchFieldId = dto.ResearchFieldId,
                Image = dto.Image,
                UpdatedBy = adminUserId
            };

            var result = await _themeApplicationService.UpdateThemeAsync(command);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating theme: {ex.Message}");
            return ErrorResponse<string>("An error occurred while updating the theme.");
        }
    }

    /// <summary>
    /// DELETE: api/Theme/{id}
    /// Delete a theme permanently (Admins only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                return ValidationProblem();
            }

            var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var command = new DeleteThemeCommand
            {
                ThemeId = id,
                DeletedBy = adminUserId
            };

            var result = await _themeApplicationService.DeleteThemeAsync(command);
            
            if (result)
            {
                return Ok(new { message = "Theme deleted successfully." });
            }
            else
            {
                return ErrorResponse<string>("Failed to delete the theme.");
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting theme: {ex.Message}");
            return ErrorResponse<string>("An error occurred while deleting the theme.");
        }
    }

    /// <summary>
    /// POST: api/Theme/{id}/publish
    /// Admin publishes an approved theme to make it visible on the public themes page.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                return ValidationProblem();
            }

            var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var command = new PublishThemeCommand
            {
                ThemeId = id,
                PublishedBy = adminUserId
            };

            var result = await _themeApplicationService.PublishThemeAsync(command);
            
            return Ok(new { message = "Theme published successfully.", theme = result });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error publishing theme: {ex.Message}");
            return ErrorResponse<string>("An error occurred while publishing the theme.");
        }
    }

    /// <summary>
    /// POST: api/Theme/{id}/unpublish
    /// Admin unpublishes a theme to hide it from the public themes page.
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError(nameof(id), "A valid theme ID is required.");
                return ValidationProblem();
            }

            var adminUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var command = new UnpublishThemeCommand
            {
                ThemeId = id,
                UnpublishedBy = adminUserId
            };

            var result = await _themeApplicationService.UnpublishThemeAsync(command);
            
            return Ok(new { message = "Theme unpublished successfully.", theme = result });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return ValidationProblem();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error unpublishing theme: {ex.Message}");
            return ErrorResponse<string>("An error occurred while unpublishing the theme.");
        }
    }

    }
}
