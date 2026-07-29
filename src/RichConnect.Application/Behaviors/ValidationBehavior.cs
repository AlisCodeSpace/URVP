using FluentValidation;
using MediatR;

namespace RICHConnect.Backend.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that automatically validates commands and queries using FluentValidation
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Skip validation if no validators are registered for this request type
            if (!_validators.Any())
            {
                _logger.LogDebug("No validators found for request type {RequestType}", typeof(TRequest).Name);
                return await next();
            }

            _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

            // Run all validators
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

            // Check if any validation failed
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning("Validation failed for request type {RequestType}. Errors: {Errors}",
                    typeof(TRequest).Name,
                    string.Join(", ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

                throw new ValidationException(failures);
            }

            _logger.LogDebug("Validation passed for request type {RequestType}", typeof(TRequest).Name);

            return await next();
        }
    }
}
