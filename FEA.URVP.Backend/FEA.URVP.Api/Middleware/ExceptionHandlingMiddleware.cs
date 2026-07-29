using System.Net;
using System.Text.Json;
using FEA.URVP.Api.Contracts;
using FluentValidation;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent JSON API response.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            throw exception;
        }

        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var includeDetails = environment.IsDevelopment();

        var (statusCode, message, errors) = MapException(exception, includeDetails);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.ErrorResult(message, errors);
        response.TraceId = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }

    private static (HttpStatusCode StatusCode, string Message, List<string>? Errors) MapException(
        Exception exception,
        bool includeDetails)
    {
        return exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationException.Errors.Select(e => e.ErrorMessage).Distinct().ToList()),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                includeDetails ? exception.Message : "Invalid request parameters",
                includeDetails ? [exception.Message] : null),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Access denied",
                null),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Resource not found",
                null),

            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                includeDetails ? exception.Message : "Invalid operation",
                includeDetails ? [exception.Message] : null),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                includeDetails
                    ? BuildDetailErrors(exception)
                    : null)
        };
    }

    private static List<string> BuildDetailErrors(Exception exception)
    {
        var errors = new List<string> { exception.Message, $"Exception type: {exception.GetType().FullName}" };

        if (exception.InnerException is not null)
        {
            errors.Add($"Inner exception: {exception.InnerException.Message}");
        }

        return errors;
    }
}
