using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Commands.Partners.UpdatePartner;
using RICHConnect.Backend.Application.Interfaces.Partners;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Application.Validators.Partners;

namespace RICHConnect.Backend.Api.Controllers.Community
{
    /// <summary>
    /// Controller for managing community partner profiles
    /// </summary>
    [ApiController]
    [Route("api/partner/profile")]
    [Authorize(Roles = "Community Partner,CommunityPartner")]
    public class PartnerProfileController : ApiControllerBase
    {
        private readonly IPartnerApplicationService _partnerApplicationService;
        private readonly UpdateCommunityPartnerDtoValidator _dtoValidator;

        public PartnerProfileController(
            IPartnerApplicationService partnerApplicationService,
            UpdateCommunityPartnerDtoValidator dtoValidator)
        {
            _partnerApplicationService = partnerApplicationService;
            _dtoValidator = dtoValidator;
        }

        /// <summary>
        /// GET: api/partner/profile
        /// Get the current user's CommunityPartner profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var partner = await _partnerApplicationService.GetPartnerByUserIdAsync(userId);
            
            if (partner == null)
                return NotFound("No CommunityPartner profile found for this user.");

            return Ok(partner);
        }

        /// <summary>
        /// PUT: api/partner/profile
        /// Update the current user's CommunityPartner profile
        /// </summary>
        [HttpPut]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit (standardized)
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateCommunityPartnerDto dto)
        {
            // Validate the DTO
            if (!TryValidate(dto, _dtoValidator))
            {
                return ValidationProblem();
            }

            try
            {
                var userId = GetCurrentUserId();

                // Map DTO to Command
                var command = new UpdatePartnerCommand
                {
                    UserId = userId,
                    Logo = dto.Logo,
                    InstitutionName = dto.InstitutionName,
                    InstitutionAddress = dto.InstitutionAddress,
                    PhoneNumber = dto.PhoneNumber,
                    RegistrationNumberArea = dto.RegistrationNumberArea,
                    ChamberOfCommerceNumber = dto.ChamberOfCommerceNumber,
                    Sector = dto.Sector,
                    InstitutionSize = dto.InstitutionSize,
                    Vision = dto.Vision,
                    Mission = dto.Mission,
                    CertificationNumber = dto.CertificationNumber,
                    AccreditationType = dto.AccreditationType
                };

                // Execute command
                var result = await _partnerApplicationService.UpdatePartnerAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return ValidationProblem();
            }
        }


    }
}
