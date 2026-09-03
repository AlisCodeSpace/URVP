using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FEA.URVP.Api.Contracts;
using FluentValidation;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into a consistent JSON API response.
/// </summary>
/// <remarks>
/// Outside Development the response carries a generic message plus the trace identifier and
/// nothing else. Stack traces, inner exceptions, SQL and identity-provider errors, and server
/// paths stay server-side, where the full exception is logged alongside a redacted message chain
/// so an operator can correlate a user's trace id with the real cause.
/// </remarks>
public sealed class ExceptionHandlingMiddleware
{
    private const int MaxLoggedMessageLength = 2000;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client hung up. There is nobody left to write an envelope to.
            _logger.LogDebug(
                "Request aborted by the client: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path}. User: {UserId}. TraceId: {TraceId}. Detail: {Detail}",
                context.Request.Method,
                context.Request.Path,
                context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
                context.TraceIdentifier,
                SecretRedactor.RedactAndTruncate(DescribeChain(ex), MaxLoggedMessageLength));

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            // Headers are already on the wire, so the envelope cannot be written. Rethrowing lets
            // the server abort the response rather than emit a half-written body.
            throw exception;
        }

        var includeDetails = _environment.IsDevelopment();
        var (statusCode, message, errors) = MapException(exception, context, includeDetails);

        context.Response.ContentLength = null;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.ErrorResult(message, errors);
        response.TraceId = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }

    private static (HttpStatusCode StatusCode, string Message, List<string>? Errors) MapException(
        Exception exception,
        HttpContext context,
        bool includeDetails)
    {
        return exception switch
        {
            // FluentValidation messages are authored for end users, so they are safe to return
            // in every environment.
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationException.Errors.Select(error => error.ErrorMessage).Distinct().ToList()),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                includeDetails ? exception.Message : "Invalid request parameters",
                includeDetails ? [exception.Message] : null),

            // Handlers raise this for ownership and role failures. An authenticated caller who
            // lacks access is forbidden, not unauthenticated; returning 401 here would make the
            // frontend discard a perfectly valid session.
            UnauthorizedAccessException => (
                context.User?.Identity?.IsAuthenticated == true
                    ? HttpStatusCode.Forbidden
                    : HttpStatusCode.Unauthorized,
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

            // Covers request bodies and multipart payloads that exceed the configured limits.
            BadHttpRequestException badRequest => (
                (HttpStatusCode)badRequest.StatusCode,
                "The request was rejected because it was malformed or too large.",
                null),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                includeDetails ? BuildDetailErrors(exception) : null)
        };
    }

    private static List<string> BuildDetailErrors(Exception exception)
    {
        var errors = new List<string>
        {
            exception.Message,
            $"Exception type: {exception.GetType().FullName}"
        };

        if (exception.InnerException is not null)
        {
            errors.Add($"Inner exception: {exception.InnerException.Message}");
        }

        return errors;
    }

    /// <summary>
    /// Flattens the message chain for the log line. The exception object is logged separately and
    /// still carries the stack trace.
    /// </summary>
    private static string DescribeChain(Exception exception)
    {
        var builder = new StringBuilder();
        var current = exception;
        var depth = 0;

        while (current is not null && depth < 5)
        {
            if (depth > 0)
            {
                builder.Append(" -> ");
            }

            builder.Append(current.GetType().Name).Append(": ").Append(current.Message);
            current = current.InnerException;
            depth++;
        }

        return builder.ToString();
    }
}
