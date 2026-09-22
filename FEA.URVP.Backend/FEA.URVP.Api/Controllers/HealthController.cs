using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Api.Controllers;

/// <summary>
/// Liveness and readiness probes.
/// </summary>
/// <remarks>
/// Liveness is public and does not touch a dependency. Readiness is not public: it requires an
/// authenticated administrator or a caller on <c>Security:Health:MonitoringNetworks</c>. The
/// database result is cached, so a probe does not open a live connection on every call.
/// </remarks>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly DatabaseReadinessCheck _readiness;
    private readonly SecurityOptions _securityOptions;

    public HealthController(
        DatabaseReadinessCheck readiness,
        IOptions<SecurityOptions> securityOptions)
    {
        _readiness = readiness;
        _securityOptions = securityOptions.Value;
    }

    /// <summary>
    /// Minimal liveness signal. Also mapped at <c>/health</c> for platform probes configured
    /// against that path.
    /// </summary>
    [HttpGet]
    [HttpHead]
    [HttpGet("live")]
    [HttpHead("live")]
    [AllowAnonymous]
    public IActionResult Live() => Ok(new { status = "healthy" });

    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        if (!IsReadyAuthorized())
        {
            return User.Identity?.IsAuthenticated == true
                ? Forbid()
                : Unauthorized();
        }

        var snapshot = await _readiness.GetAsync(cancellationToken);

        var status = snapshot.IsReachable ? "healthy" : "unhealthy";
        var statusCode = snapshot.IsReachable
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, new
        {
            status,
            checks = new[]
            {
                new
                {
                    name = "sqlserver",
                    status,
                    error = snapshot.FailureType
                }
            }
        });
    }

    private bool IsReadyAuthorized()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(nameof(UserRole.Admin)))
        {
            return true;
        }

        var networks = IpAllowList.ParseNetworks(_securityOptions.Health.MonitoringNetworks);
        return networks.Count > 0
            && IpAllowList.Contains(networks, HttpContext.Connection.RemoteIpAddress);
    }
}
