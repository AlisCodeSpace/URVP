using System.Security.Claims;
using FEA.URVP.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Base;

/// <summary>
/// Shared controller helpers for consistent responses and claim access.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult SuccessResponse<T>(T data, string message = "Operation completed successfully")
    {
        var response = ApiResponse<T>.SuccessResult(data, message);
        response.TraceId = HttpContext.TraceIdentifier;
        return Ok(response);
    }

    protected IActionResult ErrorResponse<T>(
        string message,
        IEnumerable<string>? errors = null,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        var response = ApiResponse<T>.ErrorResult(message, errors);
        response.TraceId = HttpContext.TraceIdentifier;
        return StatusCode(statusCode, response);
    }

    protected IActionResult PaginatedResponse<T>(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var payload = Contracts.PaginatedResponse<T>.Create(items, pageNumber, pageSize, totalCount);
        return SuccessResponse(payload, "Data retrieved successfully");
    }

    protected IActionResult ResourceNotFound(string resourceType, object id) =>
        ErrorResponse<object>(
            $"{resourceType} not found",
            [$"The {resourceType.ToLowerInvariant()} with ID {id} was not found"],
            StatusCodes.Status404NotFound);

    protected IActionResult UnauthorizedResponse(string message = "Access denied") =>
        ErrorResponse<object>(message, statusCode: StatusCodes.Status401Unauthorized);

    protected IActionResult ForbiddenResponse(string message = "Insufficient permissions") =>
        ErrorResponse<object>(message, statusCode: StatusCodes.Status403Forbidden);

    protected IActionResult ConflictResponse(string message = "Resource conflict") =>
        ErrorResponse<object>(message, statusCode: StatusCodes.Status409Conflict);

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    protected string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected string GetCurrentUserRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    protected bool UserHasRole(string role) => User.IsInRole(role);
}
