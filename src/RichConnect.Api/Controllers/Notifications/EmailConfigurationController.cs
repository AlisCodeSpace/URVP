using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.Notifications;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Api.Controllers.Notifications
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailConfigurationController : ApiControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly AppDbContext _dbContext;

        public EmailConfigurationController(IEmailService emailService, AppDbContext dbContext)
        {
            _emailService = emailService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get email configuration status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetEmailStatus()
        {
            var status = new
            {
                IsConfigured = _emailService.IsEmailConfigured(),
                StatusMessage = _emailService.GetEmailConfigurationStatus()
            };

            return SuccessResponse(status);
        }

        /// <summary>
        /// Get paginated email logs with optional filters
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] bool? success = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? to = null)
        {
            var query = _dbContext.EmailLogs.AsNoTracking();

            if (success.HasValue)
                query = query.Where(e => e.Success == success.Value);
            if (fromDate.HasValue)
                query = query.Where(e => e.CreatedOn >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(e => e.CreatedOn <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrWhiteSpace(to))
                query = query.Where(e => e.To.Contains(to));

            var projected = query
                .OrderByDescending(e => e.CreatedOn)
                .Select(e => new EmailLogDto
                {
                    Id = e.Id,
                    From = e.From,
                    To = e.To,
                    Success = e.Success,
                    Exception = e.Exception,
                    CreatedOn = e.CreatedOn
                });

            return await SafeExecutePaginatedAsync(projected, page, pageSize);
        }

        /// <summary>
        /// Send a test email to verify SMTP is working
        /// </summary>
        [HttpPost("test-send")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.To))
                return ErrorResponse<object>("Recipient email is required.");

            if (!_emailService.IsEmailConfigured())
                return ErrorResponse<object>("Email service is not configured.");

            var success = await _emailService.SendEmailAsync(
                request.To,
                "Test Recipient",
                "RICHConnect Email Test",
                $"This is a test email sent from the RICHConnect admin panel at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.\n\nIf you received this, SMTP is working correctly.");

            if (success)
                return SuccessResponse<object?>(null, $"Test email sent successfully to {request.To}.");

            return ErrorResponse<object>("Failed to send test email. Check email logs for details.");
        }
    }

    public class TestEmailRequest
    {
        public string To { get; set; } = string.Empty;
    }
}
