using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.DTOs.Faculty;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using AUB.APIServices.FMIS.Contracts.Interfaces;
using AUB.APIServices.FMIS.Contracts.Classes;
using ProtoBuf.Grpc;

namespace RICHConnect.Backend.Api.Controllers.FacultySpecialists
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacultySpecialistProfileController : ApiControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FacultySpecialistProfileController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IFMISService? _fmisService;

        public FacultySpecialistProfileController(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<FacultySpecialistProfileController> logger,
            IMemoryCache cache
            , IFMISService? fmisService = null
        )
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _fmisService = fmisService;
        }

        /// <summary>
        /// Get facultySpecialist profile by user ID
        /// Requires authentication - accessible to all authenticated users
        /// SECURITY: Email is only included if viewer is the owner or an admin
        /// </summary>
        /// <param name="userId">The user ID of the faculty specialist</param>
        /// <returns>Faculty specialist profile data</returns>
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<ActionResult<FacultySpecialistProfileDto>> GetFacultySpecialistProfile([FromRoute, StringLength(36, MinimumLength = 36)] string userId)
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
                return NotFound("facultySpecialist profile not found");
            }

            var fmisMember = await TrySyncFmisDataAsync(user, HttpContext.RequestAborted);

            // SECURITY: Check if viewer is owner or admin to include sensitive data
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var isAdmin = User.IsInRole("Admin");
            var isOwner = currentUserId == userGuid;
            var includeSensitiveData = isOwner || isAdmin;

            return Ok(await MapToFacultySpecialistProfileDtoAsync(user, includeSensitiveData, fmisMember, HttpContext.RequestAborted));
        }

        /// <summary>
        /// GET: api/FacultySpecialistProfile/my
        /// Get the current faculty specialist's profile
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<ActionResult<FacultySpecialistProfileDto>> GetMyProfile()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Id == userId && u.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("facultySpecialist profile not found");
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

            var fmisMember = await TrySyncFmisDataAsync(user, HttpContext.RequestAborted);

            return Ok(await MapToFacultySpecialistProfileDtoAsync(user, includeSensitiveData: true, fmisMember, HttpContext.RequestAborted));
        }

        /// <summary>
        /// PUT: api/FacultySpecialistProfile/my/status
        /// Update the current facultySpecialist's availability status
        /// </summary>
        [HttpPut("my/status")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist")]
        public async Task<ActionResult<FacultySpecialistProfileDto>> UpdateMyStatus([FromBody] UpdateFacultySpecialistStatusDto dto)
        {
            // Validate status value
            if (dto.Status != 0 && dto.Status != 1)
            {
                return BadRequest("Status must be 0 (Unavailable) or 1 (Available)");
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Id == userId && u.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("facultySpecialist profile not found");
            }

            // Create FacultySpecialist record if it doesn't exist
            if (user.FacultySpecialist == null)
            {
                _logger.LogInformation("Creating FacultySpecialist record for user {UserId}", userId);
                
                var facultySpecialist = new Domain.Entities.Faculty.FacultySpecialist
                {
                    UserId = userId,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.FacultySpecialists.Add(facultySpecialist);
                await _context.SaveChangesAsync();
                
                user.FacultySpecialist = facultySpecialist;
            }
            else
            {
                // Update the status
                user.FacultySpecialist.Status = dto.Status;
                user.FacultySpecialist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Return updated profile
            return Ok(await MapToFacultySpecialistProfileDtoAsync(user, includeSensitiveData: true, null, HttpContext.RequestAborted));
        }

        /// <summary>
        /// Update facultySpecialist status (available/unavailable)
        /// </summary>
        /// <param name="userId">The user ID of the facultySpecialist</param>
        /// <param name="dto">Status update data</param>
        /// <returns>Updated facultySpecialist profile</returns>
        [HttpPut("{userId}/status")]
        [Authorize(Roles = "Faculty Specialist,FacultySpecialist,Admin")]
        public async Task<ActionResult<FacultySpecialistProfileDto>> UpdateStatus([FromRoute, StringLength(36, MinimumLength = 36)] string userId, [FromBody] UpdateFacultySpecialistStatusDto dto)
        {
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return BadRequest("Invalid user ID format");
            }

            // Validate status value
            if (dto.Status != 0 && dto.Status != 1)
            {
                return BadRequest("Status must be 0 (Unavailable) or 1 (Available)");
            }

            // Enforce that only the owner can update their status unless the caller is Admin
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != userGuid)
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Id == userGuid && u.Role == UserRole.FacultySpecialist)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("facultySpecialist profile not found");
            }

            // Create FacultySpecialist record if it doesn't exist
            if (user.FacultySpecialist == null)
            {
                _logger.LogInformation("Creating FacultySpecialist record for user {UserId}", userGuid);
                
                var facultySpecialist = new Domain.Entities.Faculty.FacultySpecialist
                {
                    UserId = userGuid,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.FacultySpecialists.Add(facultySpecialist);
                await _context.SaveChangesAsync();
                
                user.FacultySpecialist = facultySpecialist;
            }
            else
            {
                // Update the status
                user.FacultySpecialist.Status = dto.Status;
                user.FacultySpecialist.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Return updated profile
            return Ok(await MapToFacultySpecialistProfileDtoAsync(user, includeSensitiveData: true, null, HttpContext.RequestAborted));
        }

        /// <summary>
        /// GET: api/FacultySpecialistProfile/all
        /// Get all facultySpecialist profiles (admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<FacultySpecialistProfileDto>>> GetAllFacultySpecialists()
        {
            var users = await _context.Users
                .Include(u => u.FacultySpecialist)
                .Where(u => u.Role == UserRole.FacultySpecialist)
                .ToListAsync();

            // Admins can see all sensitive data
            var mapped = new List<FacultySpecialistProfileDto>(users.Count);
            foreach (var u in users)
            {
                mapped.Add(await MapToFacultySpecialistProfileDtoAsync(u, includeSensitiveData: true, null, HttpContext.RequestAborted));
            }

            return Ok(mapped);
        }

        private static string GetDepartmentString(FacultySpecialistDepartment? department)
        {
            return department switch
            {
                FacultySpecialistDepartment.ComputerScience => "computer-science",
                FacultySpecialistDepartment.ElectricalEngineering => "electrical-engineering",
                FacultySpecialistDepartment.MechanicalEngineering => "mechanical-engineering",
                FacultySpecialistDepartment.Biology => "biology",
                FacultySpecialistDepartment.Chemistry => "chemistry",
                FacultySpecialistDepartment.Physics => "physics",
                FacultySpecialistDepartment.Mathematics => "mathematics",
                FacultySpecialistDepartment.Business => "business",
                FacultySpecialistDepartment.Medicine => "medicine",
                FacultySpecialistDepartment.Other => "other",
                _ => "other"
            };
        }

        private static string GetAcademicRankString(AcademicRank? academicRank)
        {
            return academicRank switch
            {
                AcademicRank.AssistantfacultySpecialist => "assistant-facultySpecialist",
                AcademicRank.AssociatefacultySpecialist => "associate-facultySpecialist",
                AcademicRank.facultySpecialist => "facultySpecialist",
                AcademicRank.DistinguishedfacultySpecialist => "distinguished-facultySpecialist",
                AcademicRank.Emeritus => "emeritus",
                AcademicRank.Adjunct => "adjunct",
                AcademicRank.Visiting => "visiting",
                _ => "assistant-facultySpecialist"
            };
        }

        /// <summary>
        /// Looks up the user's FMIS record by matching their email against each configured faculty list.
        /// Uses the shared per-faculty cache (5-min TTL) to avoid hammering FMIS on every profile load.
        /// Returns null if FMIS is unavailable, the user is not found, or an error occurs.
        /// </summary>
        private async Task<FacultyMemberLite?> TrySyncFmisDataAsync(User user, CancellationToken ct)
        {
            if (_fmisService == null || string.IsNullOrWhiteSpace(user.Email))
                return null;

            var email = user.Email.ToLowerInvariant().Trim();

            try
            {
                var faculties = _configuration.GetSection("Fmis:Faculties").Get<string[]>() ?? Array.Empty<string>();

                foreach (var faculty in faculties)
                {
                    try
                    {
                        var cacheKey = $"fmis_faculty_list_{faculty}";
                        if (!_cache.TryGetValue(cacheKey, out FacultyMemberLite[]? members))
                        {
                            var request = new GetFacultyMembersRequest { Faculty = faculty };
                            var callContext = new CallContext(new Grpc.Core.CallOptions(cancellationToken: ct));
                            members = await _fmisService.GetFacultyMembersInFaculty(request, callContext);
                            _cache.Set(cacheKey, members, new MemoryCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                            });
                        }

                        var match = members?.FirstOrDefault(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                            return match;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[FMIS] Error fetching faculty list for {Faculty} during profile sync", faculty);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FMIS] Unexpected error during profile FMIS sync for user {UserId}", user.Id);
            }

            return null;
        }

        /// <summary>
        /// Helper method to map User entity to FacultySpecialistProfileDto using FacultySpecialist.
        /// </summary>
        /// <param name="user">The user entity</param>
        /// <param name="includeSensitiveData">Whether to include sensitive data like email (owner/admin only)</param>
        /// <param name="fmisMember">Live FMIS data for this user, or null if unavailable</param>
        private async Task<FacultySpecialistProfileDto> MapToFacultySpecialistProfileDtoAsync(
            User user,
            bool includeSensitiveData,
            FacultyMemberLite? fmisMember,
            CancellationToken ct)
        {
            var researchInterests = user.FacultySpecialist?.ResearchInterests?.ToList() ?? new List<string>();
            var researchInterestsWithMetadata = await BuildResearchInterestsWithMetadataAsync(user.Id, researchInterests, ct);

            return new FacultySpecialistProfileDto
            {
                Id = user.Id.ToString(),
                UserId = user.Id.ToString(),
                Email = includeSensitiveData ? user.Email ?? string.Empty : string.Empty,
                PhoneNumber = string.Empty,
                Status = user.FacultySpecialist?.Status ?? 1,
                ProfilePhoto = user.ProfileImageUrl,
                FullName = user.Name,
                Department = "other",
                AcademicRank = "assistant-facultySpecialist",
                OfficeLocation = null,
                Biography = null,
                ResearchInterests = researchInterests,
                ResearchInterestsWithMetadata = researchInterestsWithMetadata,
                CreatedAt = user.FacultySpecialist?.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = user.FacultySpecialist?.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") ?? user.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),

                FmisMemberId = string.IsNullOrEmpty(fmisMember?.MemberId) ? null : fmisMember!.MemberId,
                FmisRank = string.IsNullOrEmpty(fmisMember?.Rank) ? null : fmisMember!.Rank,
                FmisDepartment = string.IsNullOrEmpty(fmisMember?.Department) ? null : fmisMember!.Department,
                FmisFaculty = string.IsNullOrEmpty(fmisMember?.Faculty) ? null : fmisMember!.Faculty,
                FmisLastSyncedAt = fmisMember != null ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : null
            };
        }

        private async Task<List<ResearchInterestDto>> BuildResearchInterestsWithMetadataAsync(
            Guid currentUserId,
            List<string> researchInterests,
            CancellationToken ct)
        {
            var result = new List<ResearchInterestDto>();
            if (researchInterests.Count == 0)
            {
                return result;
            }

            // Normalize interests for case-insensitive matching without using StringComparison in EF queries.
            var normalized = researchInterests
                .Select(i => (i ?? string.Empty).Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.ToLower())
                .Distinct()
                .ToList();

            if (normalized.Count == 0)
            {
                return result;
            }

            // Fetch only fields that match the user's interests (includes inactive fields so we can still determine editability).
            var matchingFields = await _context.ResearchFields
                .AsNoTracking()
                .Where(rf => normalized.Contains(rf.Name.ToLower()))
                .ToListAsync(ct);

            var fieldByName = matchingFields
                .GroupBy(f => f.Name.ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var interest in researchInterests)
            {
                var key = (interest ?? string.Empty).Trim().ToLower();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (fieldByName.TryGetValue(key, out ResearchField? field))
                {
                    result.Add(new ResearchInterestDto
                    {
                        Name = field.Name, // canonical name
                        CreatedBy = field.CreatedBy,
                        // Requirement: user can edit ONLY fields they submitted
                        CanEdit = field.SubmittedBy == currentUserId
                    });
                }
                else
                {
                    // If field is missing in DB, treat as read-only to avoid allowing edits of unknown provenance.
                    result.Add(new ResearchInterestDto
                    {
                        Name = (interest ?? string.Empty).Trim(),
                        CreatedBy = CreatorType.Admin,
                        CanEdit = false
                    });
                }
            }

            return result;
        }
    }
} 