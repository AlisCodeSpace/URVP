using FEA.URVP.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FEA.URVP.Api.Filters;

/// <summary>
/// Returns a consistent API envelope when ModelState is invalid.
/// </summary>
public sealed class ValidationFilter : IActionFilter
{
    private readonly ILogger<ValidationFilter> _logger;

    public ValidationFilter(ILogger<ValidationFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? $"{entry.Key} is invalid"
                    : error.ErrorMessage))
            .Distinct()
            .ToList();

        _logger.LogWarning(
            "Model validation failed for {Action}: {Errors}",
            context.ActionDescriptor.DisplayName,
            string.Join("; ", errors));

        var response = ApiResponse<object>.ErrorResult("Validation failed", errors);
        response.TraceId = context.HttpContext.TraceIdentifier;

        context.Result = new BadRequestObjectResult(response);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
