using System.Diagnostics;
using FluentValidation;
using MediatR;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Common
{
    /// <summary>
    /// Base handler for commands that return a response
    /// Provides logging, error handling, and optional transaction support
    /// </summary>
    public abstract class BaseCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        protected readonly ILogger _logger;
        protected readonly AppDbContext _context;
        
        protected BaseCommandHandler(ILogger logger, AppDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        /// <summary>
        /// Override this to disable automatic transaction management
        /// </summary>
        protected virtual bool UseTransaction => false;
        
        public virtual async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Handling {CommandType} with {HandlerType}",
                    correlationId,
                    typeof(TRequest).Name,
                    GetType().Name);
                
                TResponse response;
                
                // Execute with or without transaction
                if (UseTransaction)
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        response = await HandleInternal(request, cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }
                else
                {
                    response = await HandleInternal(request, cancellationToken);
                }
                
                stopwatch.Stop();
                _logger.LogInformation(
                    "[{CorrelationId}] Successfully handled {CommandType} in {ElapsedMs}ms",
                    correlationId,
                    typeof(TRequest).Name,
                    stopwatch.ElapsedMilliseconds);
                
                return response;
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Invalid argument in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Unauthorized access in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Invalid operation in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (ValidationException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Validation failed in {HandlerType} after {ElapsedMs}ms",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[{CorrelationId}] Unexpected error in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Implement this method with your command handling logic
        /// </summary>
        protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
    }
    
    /// <summary>
    /// Base handler for commands that don't return a response (return Unit)
    /// Provides logging, error handling, and optional transaction support
    /// </summary>
    public abstract class BaseCommandHandler<TRequest> : IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    {
        protected readonly ILogger _logger;
        protected readonly AppDbContext _context;
        
        protected BaseCommandHandler(ILogger logger, AppDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        /// <summary>
        /// Override this to enable automatic transaction management
        /// </summary>
        protected virtual bool UseTransaction => false;
        
        public virtual async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Handling {CommandType} with {HandlerType}",
                    correlationId,
                    typeof(TRequest).Name,
                    GetType().Name);
                
                // Execute with or without transaction
                if (UseTransaction)
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        await HandleInternal(request, cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                }
                else
                {
                    await HandleInternal(request, cancellationToken);
                }
                
                stopwatch.Stop();
                _logger.LogInformation(
                    "[{CorrelationId}] Successfully handled {CommandType} in {ElapsedMs}ms",
                    correlationId,
                    typeof(TRequest).Name,
                    stopwatch.ElapsedMilliseconds);

                return Unit.Value;
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Invalid argument in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Unauthorized access in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Invalid operation in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
            catch (ValidationException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex,
                    "[{CorrelationId}] Validation failed in {HandlerType} after {ElapsedMs}ms",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[{CorrelationId}] Unexpected error in {HandlerType} after {ElapsedMs}ms: {Message}",
                    correlationId,
                    GetType().Name,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Implement this method with your command handling logic
        /// </summary>
        protected abstract Task HandleInternal(TRequest request, CancellationToken cancellationToken);
    }
}
