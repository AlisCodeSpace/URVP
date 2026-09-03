using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using FEA.URVP.Api.Contracts;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Request budgets for abuse-sensitive endpoints.
/// </summary>
/// <remarks>
/// Counters are in-process. This is sound only for the documented single-instance deployment;
/// running more than one instance divides every budget by the instance count and requires a
/// distributed store instead. See <c>docs/SECURITY.md</c>.
/// <para>
/// Partitioning uses <see cref="ConnectionInfo.RemoteIpAddress"/>, which the forwarded-headers
/// middleware has already rewritten to the real client IP. The reverse proxy must therefore send
/// the same <c>X-Forwarded-For</c> chain the middleware is configured to trust, otherwise every
/// request shares one partition.
/// </para>
/// </remarks>
public static class RateLimitingConfiguration
{
    public const string AuthPolicy = "urvp-auth";
    public const string UploadPolicy = "urvp-upload";
    public const string DownloadPolicy = "urvp-download";
    public const string ReportPolicy = "urvp-report";
    public const string PublicFormPolicy = "urvp-public-form";

    private const string LoggerCategory = "FEA.URVP.Api.Security.RateLimiting";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddUrvpRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var limits = configuration
            .GetSection(SecurityOptions.SectionName)
            .Get<SecurityOptions>()?.RateLimiting
            ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = OnRejectedAsync;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    PartitionKey(context),
                    _ => FixedWindow(limits.GlobalPermitPerMinute, limits.QueueLimit)));

            AddFixedWindowPolicy(options, AuthPolicy, limits.AuthPermitPerMinute, limits.QueueLimit);
            AddFixedWindowPolicy(options, UploadPolicy, limits.UploadPermitPerMinute, limits.QueueLimit);
            AddFixedWindowPolicy(options, DownloadPolicy, limits.DownloadPermitPerMinute, limits.QueueLimit);
            AddFixedWindowPolicy(options, ReportPolicy, limits.ReportPermitPerMinute, limits.QueueLimit);
            AddFixedWindowPolicy(options, PublicFormPolicy, limits.ReportPermitPerMinute, limits.QueueLimit);
        });

        return services;
    }

    private static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        int permitPerMinute,
        int queueLimit)
    {
        options.AddPolicy(policyName, context => RateLimitPartition.GetFixedWindowLimiter(
            $"{policyName}:{PartitionKey(context)}",
            _ => FixedWindow(permitPerMinute, queueLimit)));
    }

    private static FixedWindowRateLimiterOptions FixedWindow(int permitPerMinute, int queueLimit) => new()
    {
        PermitLimit = permitPerMinute > 0 ? permitPerMinute : 1,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = queueLimit > 0 ? queueLimit : 0,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = true
    };

    /// <summary>
    /// Authenticated callers are partitioned by user id so one shared campus NAT address cannot
    /// exhaust another user's budget. Anonymous callers fall back to the real client IP.
    /// </summary>
    private static string PartitionKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        var address = context.Connection.RemoteIpAddress;
        return address is null ? "ip:unknown" : $"ip:{address}";
    }

    private static async ValueTask OnRejectedAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(LoggerCategory)
            .LogWarning(
                "Rate limit exceeded for {Method} {Path} by {Partition}. TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                PartitionKey(httpContext),
                httpContext.TraceIdentifier);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        // Deliberately generic: the response reveals neither the window nor the budget.
        var body = ApiResponse<object>.ErrorResult("Too many requests. Please try again later.");
        body.TraceId = httpContext.TraceIdentifier;

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body, SerializerOptions),
            cancellationToken);
    }
}
