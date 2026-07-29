using System.Net;
using System.Text.Json;
using RICHConnect.Backend.DTOs;

namespace RICHConnect.Backend.Api.Middleware
{
    /// <summary>
    /// Global exception handling middleware that provides consistent error responses
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log full exception details including inner exceptions
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                
                // Log inner exception if present (this is where the real error usually is)
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {InnerMessage}", ex.InnerException.Message);
                }
                
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // Only include detailed error information in Development (NOT in Staging or Production)
            var includeDetails = env.IsDevelopment();

            // Handle OIDC configuration errors similar to AzureAdSsoController
            string message;
            HttpStatusCode statusCode;
            
            if (exception is InvalidOperationException invalidOpEx && 
                (invalidOpEx.Message.Contains("authorization endpoint", StringComparison.OrdinalIgnoreCase) || 
                 invalidOpEx.Message.Contains("configuration", StringComparison.OrdinalIgnoreCase)))
            {
                // OIDC configuration error - return user-friendly message matching AzureAdSsoController
                statusCode = HttpStatusCode.BadRequest;
                message = "Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.";
            }
            else
            {
                (statusCode, message) = exception switch
                {
                    ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters"),
                    UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Access denied"),
                    // SECURITY FIX: Never expose internal exception messages in production
                    InvalidOperationException => (HttpStatusCode.BadRequest, includeDetails ? exception.Message : "Invalid operation"),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
                };
            }

            context.Response.StatusCode = (int)statusCode;

            // Include detailed errors only in Development (NOT in Staging or Production for security)
            List<string>? errors = null;
            if (includeDetails)
            {
                errors = new List<string> { exception.Message };
                
                // Include inner exception message if present (critical for debugging OIDC issues)
                if (exception.InnerException != null)
                {
                    errors.Add($"Inner exception: {exception.InnerException.Message}");
                    errors.Add($"Inner exception type: {exception.InnerException.GetType().FullName}");
                }
                
                errors.Add($"Exception type: {exception.GetType().FullName}");
            }

            var response = ApiResponseDto<object>.ErrorResult(message, errors);

            response.TraceId = context.TraceIdentifier;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}
