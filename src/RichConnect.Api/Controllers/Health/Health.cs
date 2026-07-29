using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;

namespace RICHConnect.Backend.Api.Controllers.Health
{
    /// <summary>
    /// Legacy health controller - kept for backward compatibility
    /// DEPRECATED: Use the built-in ASP.NET Core health check endpoints instead:
    /// - /health - detailed health status with all dependencies
    /// - /health/ready - readiness probe (critical dependencies only)
    /// - /health/live - liveness probe (app is running)
    /// </summary>
    [ApiController]
    [AllowAnonymous] // Health checks must be public for load balancers and monitoring
    public class Health : ApiControllerBase
    {
        [HttpGet]
        [Route("api/[controller]")]
        [Obsolete("Use /health endpoint instead for proper health checks")]
        public IActionResult Get()
        {
            return Ok(new 
            { 
                status = "healthy", 
                message = "API is running",
                deprecation = "This endpoint is deprecated. Use /health, /health/ready, or /health/live for proper health checks with dependency status."
            });
        }

        [HttpGet]
        [Route("api/[controller]/ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
