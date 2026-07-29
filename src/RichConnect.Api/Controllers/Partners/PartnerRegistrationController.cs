using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Api.Configuration.Auth;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Application.Commands.Partners.RegisterPartner;
using RICHConnect.Backend.Application.Interfaces.Partners;
using FluentValidation;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Application.Validators.Partners;

namespace RICHConnect.Backend.Api.Controllers.Community
{
    /// <summary>
    /// Controller for handling partner registration
    /// </summary>
    [ApiController]
    [Route("api/partner")]
    public class PartnerRegistrationController : ApiControllerBase
    {
        private readonly IPartnerApplicationService _partnerApplicationService;
        private readonly CreatePartnerDtoValidator _dtoValidator;
        private readonly ILogger<PartnerRegistrationController> _logger;

        public PartnerRegistrationController(
            IPartnerApplicationService partnerApplicationService,
            CreatePartnerDtoValidator dtoValidator,
            ILogger<PartnerRegistrationController> logger)
        {
            _partnerApplicationService = partnerApplicationService;
            _dtoValidator = dtoValidator;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/partner/register
        /// Body: multipart/form-data with fields from CreatePartnerDto including an optional logo file
        /// Authorization: Cookie-based authentication required
        /// Registers a new Community Partner
        /// </summary>
        [HttpPost("register")]
        [Authorize(AuthenticationSchemes = AuthenticationConfiguration.CookieScheme)]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit (standardized)
        public async Task<IActionResult> Register([FromForm] CreatePartnerDto dto)
        {
            // Validate the DTO
            if (!TryValidate(dto, _dtoValidator))
            {
                return ValidationProblem();
            }

            try
            {
                // Get userId from claims (cookie-based authentication)
                // Try multiple claim types for compatibility
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("nameid")?.Value 
                    ?? User.FindFirst("userId")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("User authenticated but no valid user ID found in claims");
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Invalid authentication",
                        Detail = "User ID not found in authentication claims.",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                
                _logger.LogInformation("User authenticated via cookie. User ID: {UserId}, Email: {Email}, Role: {Role}", 
                    userId, userEmail, roleClaim);

                // Map DTO to Command
                var command = new RegisterPartnerCommand
                {
                    UserId = userId,
                    Logo = dto.Logo,
                    InstitutionName = dto.InstitutionName,
                    InstitutionAddress = dto.InstitutionAddress,
                    PhoneNumber = dto.PhoneNumber,
                    RegistrationNumberArea = dto.RegistrationNumberArea,
                    Sector = dto.Sector,
                    InstitutionSize = dto.InstitutionSize,
                    ChamberOfCommerceNumber = dto.ChamberOfCommerceNumber,
                    Vision = dto.Vision,
                    Mission = dto.Mission,
                    CertificationNumber = dto.CertificationNumber,
                    AccreditationType = dto.AccreditationType
                };

                // Execute command
                var result = await _partnerApplicationService.RegisterPartnerAsync(command);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                foreach (var error in ex.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
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
                Console.Error.WriteLine($"Error registering partner: {ex.Message}");
                
                ModelState.AddModelError("", "An error occurred while registering the partner. Please try again.");
                return ValidationProblem();
            }
        }

        /// <summary>
        /// GET: api/partner/register/{id}
        /// Get registration details by ID (for confirmation)
        /// Only accessible by the owner or admin
        /// </summary>
        [HttpGet("register/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var partner = await _partnerApplicationService.GetPartnerByIdAsync(id);
            if (partner == null)
            {
                return NotFound(new { message = "Registration not found" });
            }
            
            // Authorization check: only owner or admin can view
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && partner.UserId != userId)
            {
                return Forbid();
            }
            
            return Ok(new { 
                message = "Registration successful. Your account is pending admin approval.",
                registrationId = id,
                partner = partner
            });
        }

        /// <summary>
        /// GET: api/partner/sectors
        /// Get all available institution sectors for dropdown
        /// </summary>
        [HttpGet("sectors")]
        public IActionResult GetSectors()
        {
            var sectors = Enum.GetValues(typeof(InstitutionSector))
                .Cast<InstitutionSector>()
                .Select(s => new
                {
                    Value = (int)s,
                    Name = s.ToString()
                })
                .ToList();

            return Ok(sectors);
        }

        /// <summary>
        /// GET: api/partner/sizes
        /// Get all available institution sizes for dropdown with human-readable values
        /// </summary>
        [HttpGet("sizes")]
        public IActionResult GetSizes()
        {
            var sizes = new[]
            {
                new { Value = (int)InstitutionSize.OneToTen, Name = "1-10" },
                new { Value = (int)InstitutionSize.ElevenToFifty, Name = "11-50" },
                new { Value = (int)InstitutionSize.FiftyOneToHundred, Name = "51-100" },
                new { Value = (int)InstitutionSize.HundredOneToFiveHundred, Name = "101-500" },
                new { Value = (int)InstitutionSize.FiveHundredOneToThousand, Name = "501-1000" },
                new { Value = (int)InstitutionSize.OverThousand, Name = "1000+" }
            };

            return Ok(sizes);
        }

        /// <summary>
        /// GET: api/partner/accreditation-types
        /// Get all available accreditation types for dropdown
        /// </summary>
        [HttpGet("accreditation-types")]
        public IActionResult GetAccreditationTypes()
        {
            var accreditationTypes = Enum.GetValues(typeof(AccreditationType))
                .Cast<AccreditationType>()
                .Select(at => new
                {
                    Value = (int)at,
                    Name = at.ToString()
                })
                .ToList();

            return Ok(accreditationTypes);
        }

    }
}
