using System.Text;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Controllers.Security;

/// <summary>
/// Ingests Content-Security-Policy violation reports.
/// </summary>
/// <remarks>
/// Anonymous and unauthenticated by necessity: the browser posts these directly and cannot attach
/// a session cookie or an antiforgery header. The endpoint is therefore treated as hostile input.
/// It is rate-limited, hard size-capped, and the body is redacted, stripped of control characters
/// and truncated before it reaches the log.
/// </remarks>
[ApiController]
[Route("api/security")]
public sealed class CspReportController : ControllerBase
{
    private const int MaxRequestBytes = 8 * 1024;
    private const int MaxLoggedCharacters = 2000;

    private readonly ILogger<CspReportController> _logger;

    public CspReportController(ILogger<CspReportController> logger)
    {
        _logger = logger;
    }

    [HttpPost("csp-report")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingConfiguration.ReportPolicy)]
    [RequestSizeLimit(MaxRequestBytes)]
    [Consumes("application/csp-report", "application/json", "application/reports+json")]
    public async Task<IActionResult> Report(CancellationToken cancellationToken)
    {
        var body = await ReadCappedBodyAsync(cancellationToken);

        if (body.Length > 0)
        {
            _logger.LogWarning(
                "CSP violation reported. UserAgent: {UserAgent}. Report: {Report}",
                SecretRedactor.RedactAndTruncate(Request.Headers.UserAgent.ToString(), 200),
                SecretRedactor.RedactAndTruncate(body, MaxLoggedCharacters));
        }

        // No content, so a report storm cannot be amplified into a useful response.
        return NoContent();
    }

    /// <summary>
    /// Reads at most <see cref="MaxRequestBytes"/> regardless of the declared Content-Length, so a
    /// lying header cannot cause an unbounded allocation.
    /// </summary>
    private async Task<string> ReadCappedBodyAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxRequestBytes];
        var total = 0;

        while (total < buffer.Length)
        {
            var read = await Request.Body.ReadAsync(
                buffer.AsMemory(total, buffer.Length - total),
                cancellationToken);

            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total == 0 ? string.Empty : Encoding.UTF8.GetString(buffer, 0, total);
    }
}
