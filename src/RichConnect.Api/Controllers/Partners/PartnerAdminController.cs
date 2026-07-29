using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Commands.Partners.ApprovePartner;
using RICHConnect.Backend.Application.Commands.Partners.RejectPartner;
using RICHConnect.Backend.Application.Interfaces.Partners;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Application.Validators.Partners;

namespace RICHConnect.Backend.Api.Controllers.Admin
{
    /// <summary>
    /// Controller for admin operations on community partners
    /// </summary>
    [ApiController]
    [Route("api/admin/partners")]
    [Authorize(Roles = "Admin")]
    public class PartnerAdminController : ApiControllerBase
    {
        private readonly IPartnerApplicationService _partnerApplicationService;
        private readonly RejectCommunityPartnerDtoValidator _rejectValidator;
        private readonly ILogger<PartnerAdminController> _logger;

        public PartnerAdminController(
            IPartnerApplicationService partnerApplicationService,
            RejectCommunityPartnerDtoValidator rejectValidator,
            ILogger<PartnerAdminController> logger)
        {
            _partnerApplicationService = partnerApplicationService;
            _rejectValidator = rejectValidator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: api/admin/partners/pending
        /// List all CommunityPartners with Status = Pending
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var partners = await _partnerApplicationService.GetPartnersByStatusAsync(
                ApprovalStatus.Pending);
            return Ok(partners);
        }

        /// <summary>
        /// GET: api/admin/partners/all
        /// List all CommunityPartners regardless of status
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var partners = await _partnerApplicationService.GetPartnersByStatusAsync();
            return Ok(partners);
        }

        /// <summary>
        /// GET: api/admin/partners/approved
        /// List all CommunityPartners with Status = Approved
        /// </summary>
        [HttpGet("approved")]
        public async Task<IActionResult> GetApproved()
        {
            var partners = await _partnerApplicationService.GetPartnersByStatusAsync(
                ApprovalStatus.Approved);
            return Ok(partners);
        }

        /// <summary>
        /// GET: api/admin/partners/rejected
        /// List all CommunityPartners with Status = Rejected
        /// </summary>
        [HttpGet("rejected")]
        public async Task<IActionResult> GetRejected()
        {
            var partners = await _partnerApplicationService.GetPartnersByStatusAsync(
                ApprovalStatus.Rejected);
            return Ok(partners);
        }

        /// <summary>
        /// POST: api/admin/partners/{id}/approve
        /// Admin approves a pending CommunityPartner
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                var command = new ApprovePartnerCommand
                {
                    PartnerId = id,
                    AdminUserId = adminUserId
                };

                var result = await _partnerApplicationService.ApprovePartnerAsync(command);
                return Ok(new { message = "Partner approved successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in Approve partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid request data" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Approve partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "An error occurred while processing your request.", traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// POST: api/admin/partners/{id}/reject
        /// Admin rejects a pending CommunityPartner with a required reason
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectCommunityPartnerDto dto)
        {
            // Validate the request
            if (!TryValidate(dto, _rejectValidator))
            {
                return ValidationProblem();
            }

            try
            {
                var adminUserId = GetCurrentUserId();
                var command = new RejectPartnerCommand
                {
                    PartnerId = id,
                    AdminUserId = adminUserId,
                    RejectionReason = dto.RejectionReason.Trim()
                };

                var result = await _partnerApplicationService.RejectPartnerAsync(command);
                return Ok(new { message = "Partner rejected successfully." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in Reject partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid request data" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in Reject partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "Invalid operation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting partner for PartnerId: {PartnerId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
                return BadRequest(new { message = "An error occurred while processing your request.", traceId = HttpContext.TraceIdentifier });
            }
        }



    }
}
