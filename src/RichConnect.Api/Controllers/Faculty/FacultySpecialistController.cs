using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.DTOs.Faculty;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.FacultySpecialists
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacultySpecialistController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FacultySpecialistController> _logger;

        public FacultySpecialistController(
            AppDbContext context,
            ILogger<FacultySpecialistController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get faculty specialist profile by user ID
        /// Requires authentication - accessible to all authenticated users
        /// </summary>
        /// <param name="userId">The user ID of the faculty specialist</param>
        /// <returns>Faculty specialist profile data</returns>
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<FacultySpecialistDto>> GetFacultySpecialistProfile([FromRoute, StringLength(36, MinimumLength = 36)] string userId)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("Invalid user ID format");
            }

            var user = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Id == userGuid && u.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Faculty specialist profile not found");
            }

            return Ok(MapToFacultySpecialistDto(user));
        }

        /// <summary>
        /// GET: api/FacultySpecialist/my
        /// Get the current faculty specialist's profile
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<ActionResult<FacultySpecialistDto>> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Id == userId && u.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Faculty specialist profile not found");
            }

            // Create FacultySpecialist record if it doesn't exist
            if (user.FacultySpecialist == null)
            {
                _logger.LogInformation("Creating FacultySpecialist record for user {UserId}", userId);
                
                var facultySpecialist = new Domain.Entities.Faculty.FacultySpecialist
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.FacultySpecialists.Add(facultySpecialist);
                await _context.SaveChangesAsync();
                
                user.FacultySpecialist = facultySpecialist;
            }

            return Ok(MapToFacultySpecialistDto(user));
        }

        /// <summary>
        /// PUT: api/FacultySpecialist/my
        /// Update the current faculty specialist's profile (research interests)
        /// </summary>
        [HttpPut("my")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<ActionResult<FacultySpecialistDto>> UpdateMyProfile([FromBody] UpdateFacultySpecialistDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var facultySpecialist = await _context.FacultySpecialists
                .Include(fs => fs.User)
                .Include(fs => fs.ResearchFieldLinks!)
                    .ThenInclude(rfl => rfl.ResearchField)
                .Where(fs => fs.UserId == userId && fs.User.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (facultySpecialist == null)
            {
                // Create FacultySpecialist record if it doesn't exist
                _logger.LogInformation("Creating FacultySpecialist record for user {UserId}", userId);
                
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.Role == UserRole.FacultySpecialist)
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    return NotFound("User not found or not a faculty specialist");
                }
                
                facultySpecialist = new Domain.Entities.Faculty.FacultySpecialist
                {
                    UserId = userId,
                    User = user,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.FacultySpecialists.Add(facultySpecialist);
            }

            // Process research interests and link to research fields
            await ProcessResearchInterestsAsync(facultySpecialist, dto.ResearchInterests, userId, dto.CustomResearchInterestsWithCategories);

            facultySpecialist.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return updated profile
            return Ok(MapToFacultySpecialistDto(facultySpecialist.User));
        }

        /// <summary>
        /// Process research interests: create/link research fields as needed
        /// </summary>
        private async Task ProcessResearchInterestsAsync(
            Domain.Entities.Faculty.FacultySpecialist facultySpecialist, 
            List<string> newInterests, 
            Guid userId,
            Dictionary<string, string>? customInterestsWithCategories = null)
        {
            // Get all existing research fields (case-insensitive matching)
            var existingFields = await _context.ResearchFields
                .Where(rf => rf.IsActive)
                .ToListAsync();

            var fieldsToLink = new List<Guid>();
            var interestsToStore = new List<string>();

            foreach (var interest in newInterests.Select(i => i.Trim()).Where(i => !string.IsNullOrEmpty(i)))
            {
                // Check if this interest matches an existing research field (case-insensitive)
                // Note: Data is already in memory from ToListAsync(), so we can use ToLower() for comparison
                var existingField = existingFields.FirstOrDefault(rf => 
                    rf.Name.ToLower() == interest.ToLower());

                if (existingField != null)
                {
                    // Link to existing field
                    fieldsToLink.Add(existingField.Id);
                    interestsToStore.Add(existingField.Name); // Use the canonical name from the database
                }
                else
                {
                    // Get category if this is a custom interest
                    string? category = null;
                    if (customInterestsWithCategories != null && 
                        customInterestsWithCategories.TryGetValue(interest, out var cat))
                    {
                        category = cat;
                    }
                    
                    // Create new research field with CreatedBy = Faculty
                    var newField = new Domain.Entities.ResearchFields.ResearchField
                    {
                        Name = interest,
                        Slug = GenerateSlug(interest),
                        Category = category,
                        IsActive = true,
                        Status = ApprovalStatus.Approved, // Auto-approve faculty-created fields
                        CreatedBy = CreatorType.Faculty,
                        SubmittedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _context.ResearchFields.Add(newField);
                    await _context.SaveChangesAsync(); // Save to get the ID
                    
                    fieldsToLink.Add(newField.Id);
                    interestsToStore.Add(interest);
                    
                    _logger.LogInformation("Created new research field '{FieldName}' with category '{Category}' by faculty user {UserId}", 
                        interest, category ?? "None", userId);
                }
            }

            // Update the ResearchInterestsJson field (for backward compatibility)
            facultySpecialist.ResearchInterests = interestsToStore.ToArray();

            // Update the junction table links
            // Remove old links
            var existingLinks = await _context.FacultySpecialistResearchFields
                .Where(fsrf => fsrf.FacultySpecialistUserId == facultySpecialist.UserId)
                .ToListAsync();
            
            _context.FacultySpecialistResearchFields.RemoveRange(existingLinks);

            // Add new links (avoiding duplicates)
            foreach (var fieldId in fieldsToLink.Distinct())
            {
                _context.FacultySpecialistResearchFields.Add(new Domain.Entities.ResearchFields.FacultySpecialistResearchField
                {
                    FacultySpecialistUserId = facultySpecialist.UserId,
                    ResearchFieldId = fieldId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Generate a URL-friendly slug from a research field name
        /// </summary>
        private static string GenerateSlug(string name)
        {
            return name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace(".", "");
        }

        /// <summary>
        /// GET: api/FacultySpecialist/all
        /// Get all faculty specialist profiles (admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<FacultySpecialistDto>>> GetAllFacultySpecialists()
        {
            var users = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Role == UserRole.FacultySpecialist)
                .ToListAsync();

            var facultySpecialists = users.Select(MapToFacultySpecialistDto).ToList();

            return Ok(facultySpecialists);
        }

        /// <summary>
        /// Helper method to map User entity to FacultySpecialistDto
        /// </summary>
        private FacultySpecialistDto MapToFacultySpecialistDto(User user)
        {
            var researchInterests = user.FacultySpecialist?.ResearchInterests?.ToList() ?? new List<string>();
            var researchInterestsWithMetadata = new List<Application.DTOs.Faculty.ResearchInterestDto>();

            // Load all active research fields once to avoid N+1 queries
            var allResearchFields = _context.ResearchFields
                .Where(rf => rf.IsActive)
                .ToList();

            // Get research field information for each interest
            foreach (var interest in researchInterests)
            {
                // Case-insensitive comparison in memory (data already loaded)
                var field = allResearchFields
                    .FirstOrDefault(rf => rf.Name.ToLower() == interest.ToLower());

                if (field != null)
                {
                    researchInterestsWithMetadata.Add(new Application.DTOs.Faculty.ResearchInterestDto
                    {
                        Name = field.Name, // Use canonical name from database
                        CreatedBy = field.CreatedBy,
                        // Requirement: user can edit ONLY fields they submitted
                        CanEdit = field.SubmittedBy == user.Id
                    });
                }
                else
                {
                    // If field doesn't exist in DB (shouldn't happen normally), treat as editable
                    researchInterestsWithMetadata.Add(new Application.DTOs.Faculty.ResearchInterestDto
                    {
                        Name = interest,
                        CreatedBy = CreatorType.Admin,
                        CanEdit = false
                    });
                }
            }

            return new FacultySpecialistDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FullName = user.Name,
#pragma warning disable CS0618 // Keep for backward compatibility
                ResearchInterests = researchInterests, // Keep for backward compatibility
#pragma warning restore CS0618
                ResearchInterestsWithMetadata = researchInterestsWithMetadata,
                CreatedAt = user.FacultySpecialist?.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = user.FacultySpecialist?.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? user.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }
    }

    /// <summary>
    /// DTO for updating faculty specialist profile
    /// </summary>
    public class UpdateFacultySpecialistDto
    {
        public List<string> ResearchInterests { get; set; } = new List<string>();
        
        /// <summary>
        /// Dictionary mapping custom research interest names to their categories
        /// Only used when creating new research fields (not for existing ones)
        /// </summary>
        public Dictionary<string, string>? CustomResearchInterestsWithCategories { get; set; }
    }
}
