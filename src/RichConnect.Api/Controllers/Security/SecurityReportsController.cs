using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RICHConnect.Backend.Api.Controllers.Security
{
    [ApiController]
    [Route("api/security")]
    public class SecurityReportsController : ControllerBase
    {
        private readonly ILogger<SecurityReportsController> _logger;

        public SecurityReportsController(ILogger<SecurityReportsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Receives CSP violation reports (Report-To / Reporting-Endpoints).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("csp-report")]
        public async Task<IActionResult> ReceiveCspReport()
        {
            using var reader = new StreamReader(Request.Body);
            var reportBody = await reader.ReadToEndAsync();

            if (!string.IsNullOrWhiteSpace(reportBody))
            {
                _logger.LogWarning("CSP violation report received: {ReportBody}", reportBody);
            }

            return NoContent();
        }
    }
}
