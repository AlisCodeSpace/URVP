using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Api.Controllers;

/// <summary>
/// Liveness and readiness probes.
/// </summary>
/// <remarks>
/// Liveness is public and says nothing beyond "this process is answering". Readiness performs a
/// dependency check, but only an authenticated administrator or a caller from a configured
/// monitoring network sees which dependency failed. Everyone else receives the healthy/unhealthy
/// state alone, which keeps load balancer probes working without disclosing topology.
/// </remarks>
[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private static readonly TimeSpan DependencyProbeTimeout = TimeSpan.FromSeconds(5);

    private readonly AppDbContext _dbContext;
    private readonly SecurityOptions _securityOptions;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AppDbContext dbContext,
        IOptions<SecurityOptions> securityOptions,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _securityOptions = securityOptions.Value;
        _logger = logger;
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
        var (isReachable, failureType) = await CheckDatabaseAsync(cancellationToken);

        var status = isReachable ? "healthy" : "unhealthy";
        var statusCode = isReachable
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        if (!IsDetailAuthorized())
        {
            return StatusCode(statusCode, new { status });
        }

        return StatusCode(statusCode, new
        {
            status,
            checks = new[]
            {
                new
                {
                    name = "sqlserver",
                    status,
                    // Exception type only. The message can carry the server name, database name
                    // or credentials from the connection string.
                    error = failureType
                }
            }
        });
    }

    private async Task<(bool IsReachable, string? FailureType)> CheckDatabaseAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(DependencyProbeTimeout);

            var canConnect = await _dbContext.Database.CanConnectAsync(timeout.Token);
            return (canConnect, canConnect ? null : "ConnectionRefused");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness probe: database connectivity check failed.");
            return (false, ex.GetType().Name);
        }
    }

    private bool IsDetailAuthorized()
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
