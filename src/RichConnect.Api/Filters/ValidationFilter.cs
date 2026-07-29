using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RICHConnect.Backend.DTOs;

namespace RICHConnect.Backend.Api.Filters
{
    /// <summary>
    /// Global validation filter that provides consistent validation error responses
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );

                _logger.LogWarning("Validation failed for {Action}: {Errors}", 
                    context.ActionDescriptor.DisplayName, 
                    string.Join(", ", errors.SelectMany(e => e.Value)));

                var response = ApiResponseDto<object>.ErrorResult(
                    "Validation failed",
                    errors.SelectMany(e => e.Value).ToList()
                );

                response.TraceId = context.HttpContext.TraceIdentifier;

                context.Result = new BadRequestObjectResult(response);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // This method is called after the action is executed
            // We can add post-processing logic here if needed
        }
    }
}
