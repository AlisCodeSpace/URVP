using Hangfire.Dashboard;
using System.Security.Claims;

namespace RICHConnect.Backend.Api.Security
{
    /// <summary>
    /// Authorization filter for securing the Hangfire Dashboard.
    ///
    /// Rules:
    /// - In all environments, user must be authenticated.
    /// - Prefer a role-based restriction via the "role" claim (e.g., "Admin").
    /// - If no explicit admin role is configured, only allow local requests (loopback) as a safe default.
    ///
    /// NOTE: This is intentionally conservative. If your deployment uses a different admin
    /// role name or claim type, adjust the checks below accordingly.
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private const string AdminRoleName = "Admin";

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Always require an authenticated user
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Prefer explicit admin role check
            if (httpContext.User.IsInRole(AdminRoleName))
            {
                return true;
            }

            // Some IdPs emit roles as a "roles" or "role" claim without IsInRole mapping.
            // Fall back to checking raw claims for flexibility.
            var roleClaims = httpContext.User.Claims
                .Where(c =>
                    c.Type == ClaimTypes.Role ||
                    string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (roleClaims.Contains(AdminRoleName))
            {
                return true;
            }

            // As an additional safety net, allow local requests (e.g., for dev / on-box diagnostics)
            // when not explicitly marked as admin.
            if (httpContext.Connection.RemoteIpAddress is { } remoteIp &&
                System.Net.IPAddress.IsLoopback(remoteIp))
            {
                return true;
            }

            // Otherwise deny access
            return false;
        }
    }
}

