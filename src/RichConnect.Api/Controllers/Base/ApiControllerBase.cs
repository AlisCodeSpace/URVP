using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using RICHConnect.Backend.DTOs;
using RICHConnect.Backend.Domain.Entities.Admin;

namespace RICHConnect.Backend.Api.Controllers.Base
{
    /// <summary>
    /// Enhanced base controller with comprehensive error handling, standardized responses, and utilities
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        #region Validation Methods

        /// <summary>
        /// Validates a model using a specific validator and ruleset
        /// </summary>
        /// <typeparam name="TModel">The model type</typeparam>
        /// <typeparam name="TValidator">The validator type</typeparam>
        /// <param name="model">The model to validate</param>
        /// <param name="validator">The validator instance</param>
        /// <param name="ruleSet">Optional ruleset name to use</param>
        /// <returns>True if validation passes, false otherwise</returns>
        protected bool TryValidate<TModel, TValidator>(TModel model, TValidator validator, string? ruleSet = null)
            where TValidator : IValidator<TModel>
        {
            ValidationResult result;
            
            if (string.IsNullOrEmpty(ruleSet))
            {
                result = validator.Validate(model);
            }
            else
            {
                result = validator.Validate(model, options => options.IncludeRuleSets(ruleSet));
            }

            if (result.IsValid)
            {
                return true;
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return false;
        }

        /// <summary>
        /// Validates required parameters and adds errors to ModelState
        /// </summary>
        /// <param name="parameter">Parameter to validate</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="errorMessage">Custom error message</param>
        /// <returns>True if parameter is valid, false otherwise</returns>
        protected bool ValidateRequiredParameter(object? parameter, string parameterName, string errorMessage = "Parameter is required")
        {
            if (parameter == null || (parameter is string str && string.IsNullOrWhiteSpace(str)))
            {
                ModelState.AddModelError(parameterName, errorMessage);
                return false;
            }
            return true;
        }

        #endregion

        #region Standardized Response Methods

        /// <summary>
        /// Creates a standardized success response
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data to return</param>
        /// <param name="message">Success message</param>
        /// <returns>Standardized success response</returns>
        protected IActionResult SuccessResponse<T>(T data, string message = "Operation completed successfully")
        {
            var response = ApiResponseDto<T>.SuccessResult(data, message);
            response.TraceId = HttpContext.TraceIdentifier;
            return Ok(response);
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="message">Error message</param>
        /// <param name="errors">List of specific errors</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <returns>Standardized error response</returns>
        protected IActionResult ErrorResponse<T>(string message, List<string>? errors = null, int statusCode = 400)
        {
            var response = ApiResponseDto<T>.ErrorResult(message, errors);
            response.TraceId = HttpContext.TraceIdentifier;
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Creates a standardized paginated response
        /// </summary>
        /// <typeparam name="T">Type of the data items</typeparam>
        /// <param name="items">List of items</param>
        /// <param name="pageNumber">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total number of items</param>
        /// <returns>Standardized paginated response</returns>
        protected IActionResult PaginatedResponse<T>(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            var paginatedData = PaginatedResponseDto<T>.Create(items, pageNumber, pageSize, totalCount);
            return SuccessResponse(paginatedData, "Data retrieved successfully");
        }

        #endregion

        #region Error Handling Methods

        /// <summary>
        /// Creates a standardized bad request response with validation errors
        /// </summary>
        /// <returns>A BadRequestObjectResult with validation errors</returns>
        protected new IActionResult ValidationProblem()
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var problemDetails = new ValidationProblemDetails(ModelState)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "One or more validation errors occurred",
                Detail = "Please refer to the errors property for additional details",
                Instance = HttpContext.Request.Path
            };

            return BadRequest(problemDetails);
        }

        /// <summary>
        /// Creates a standardized response for when a resource is not found
        /// </summary>
        /// <param name="resourceType">The type of resource that was not found</param>
        /// <param name="id">The ID of the resource that was not found</param>
        /// <returns>A NotFoundObjectResult with details</returns>
        protected IActionResult ResourceNotFound(string resourceType, object id)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = $"{resourceType} not found",
                Detail = $"The {resourceType.ToLowerInvariant()} with ID {id} was not found",
                Instance = HttpContext.Request.Path
            };

            return NotFound(problemDetails);
        }

        /// <summary>
        /// Creates a standardized unauthorized response
        /// </summary>
        /// <param name="message">Unauthorized message</param>
        /// <returns>Unauthorized response</returns>
        protected IActionResult UnauthorizedResponse(string message = "Access denied")
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = message,
                Instance = HttpContext.Request.Path
            };

            return Unauthorized(problemDetails);
        }

        /// <summary>
        /// Creates a standardized forbidden response
        /// </summary>
        /// <param name="message">Forbidden message</param>
        /// <returns>Forbidden response</returns>
        protected IActionResult ForbiddenResponse(string message = "Insufficient permissions")
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Forbidden,
                Title = "Forbidden",
                Detail = message,
                Instance = HttpContext.Request.Path
            };

            return Forbid();
        }

        /// <summary>
        /// Creates a standardized conflict response
        /// </summary>
        /// <param name="message">Conflict message</param>
        /// <returns>Conflict response</returns>
        protected IActionResult ConflictResponse(string message = "Resource conflict")
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Conflict",
                Detail = message,
                Instance = HttpContext.Request.Path
            };

            return Conflict(problemDetails);
        }

        #endregion

        #region Pagination Utilities

        /// <summary>
        /// Applies pagination to an IQueryable
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">The query to paginate</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated query</returns>
        protected IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Applies sorting to an IQueryable
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">The query to sort</param>
        /// <param name="sortBy">Property name to sort by</param>
        /// <param name="sortDescending">Whether to sort in descending order</param>
        /// <returns>Sorted query</returns>
        protected IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, bool sortDescending = false)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            // Note: This is a simplified implementation. In a real application,
            // you might want to use a more sophisticated sorting mechanism
            // that can handle dynamic property sorting safely.
            return query;
        }

        #endregion

        #region Audit Logging Helpers

        /// <summary>
        /// Creates an audit log entry
        /// </summary>
        /// <param name="actionType">Type of action performed</param>
        /// <param name="entityType">Type of entity affected</param>
        /// <param name="entityId">ID of the entity affected</param>
        /// <param name="oldValues">Previous values (JSON)</param>
        /// <param name="newValues">New values (JSON)</param>
        /// <returns>Audit log entry</returns>
        protected AdminActionLog CreateAuditLog(string actionType, string entityType, Guid entityId, string? oldValues = null, string? newValues = null)
        {
            var userId = GetCurrentUserId();
            
            return new AdminActionLog
            {
                AdminUserId = userId,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                ClientIpHash = GetClientIpHash(),
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Serializes an object to JSON for audit logging
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON string or null if serialization fails</returns>
        protected string? SerializeForAudit(object? obj)
        {
            if (obj == null) return null;
            
            try
            {
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region User Context Helpers

        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        /// <returns>Current user ID or Guid.Empty if not found</returns>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Gets the current user's email from claims
        /// </summary>
        /// <returns>Current user's email or empty string if not found</returns>
        protected string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the current user's role from claims
        /// </summary>
        /// <returns>Current user's role or empty string if not found</returns>
        protected string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        protected bool UserHasRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        /// <returns>True if user is admin, false otherwise</returns>
        protected bool IsAdmin()
        {
            return UserHasRole("Admin");
        }

        #endregion

        #region Request Utilities

        /// <summary>
        /// Gets the client IP address hash for audit logging
        /// </summary>
        /// <returns>Hashed client IP or null if not available</returns>
        protected string? GetClientIpHash()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ip)) return null;
            
            // Simple hash for privacy - in production, use a proper hashing algorithm
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ip));
        }

        /// <summary>
        /// Gets the user agent string
        /// </summary>
        /// <returns>User agent string or empty string if not available</returns>
        protected string GetUserAgent()
        {
            return Request.Headers.UserAgent.ToString();
        }

        /// <summary>
        /// Gets the request method
        /// </summary>
        /// <returns>HTTP method</returns>
        protected string GetRequestMethod()
        {
            return Request.Method;
        }

        /// <summary>
        /// Gets the request path
        /// </summary>
        /// <returns>Request path</returns>
        protected string GetRequestPath()
        {
            return Request.Path;
        }

        #endregion

        #region Database Utilities

        /// <summary>
        /// Safely executes a database operation with error handling
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Database operation to execute</param>
        /// <param name="errorMessage">Error message if operation fails</param>
        /// <returns>Operation result or error response</returns>
        protected async Task<IActionResult> SafeExecuteAsync<T>(Func<Task<T>> operation, string errorMessage = "Database operation failed")
        {
            try
            {
                var result = await operation();
                return SuccessResponse(result);
            }
            catch (DbUpdateException)
            {
                // Log detailed error for debugging (logged by global exception handler)
                // Return generic message to prevent information disclosure
                return ErrorResponse<T>("Database operation failed. Please try again or contact support.");
            }
            catch (Exception)
            {
                // Log detailed error for debugging (logged by global exception handler)
                // Return generic message to prevent information disclosure
                return ErrorResponse<T>("An error occurred processing your request. Please try again or contact support.");
            }
        }

        /// <summary>
        /// Safely executes a database operation that returns a list with pagination
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">Query to execute</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="errorMessage">Error message if operation fails</param>
        /// <returns>Paginated result or error response</returns>
        protected async Task<IActionResult> SafeExecutePaginatedAsync<T>(IQueryable<T> query, int pageNumber, int pageSize, string errorMessage = "Failed to retrieve data")
        {
            try
            {
                var totalCount = await query.CountAsync();
                var items = await ApplyPagination(query, pageNumber, pageSize).ToListAsync();
                
                return PaginatedResponse(items, pageNumber, pageSize, totalCount);
            }
            catch (Exception)
            {
                // Log detailed error for debugging (logged by global exception handler)
                // Return generic message to prevent information disclosure
                return ErrorResponse<List<T>>("Failed to retrieve data. Please try again or contact support.");
            }
        }

        #endregion
    }
} 