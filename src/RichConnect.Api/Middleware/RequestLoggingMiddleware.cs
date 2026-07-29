using System.Diagnostics;

namespace RICHConnect.Backend.Api.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestTime = DateTime.UtcNow;

            // Log the incoming request
            _logger.LogInformation(
                "Request {Method} {Path} started at {RequestTime}",
                context.Request.Method,
                context.Request.Path,
                requestTime
            );

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log the completed request
                _logger.LogInformation(
                    "Request {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );
            }
        }
    }
}
