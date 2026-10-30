using System.Diagnostics;
using FEA.URVP.Application.Abstractions.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Base;

/// <summary>
/// Base handler for commands that return a response.
/// Provides correlation logging and optional transactional execution.
/// </summary>
public abstract class BaseCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly ILogger Logger;
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseCommandHandler(ILogger logger, IUnitOfWork unitOfWork)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Override to wrap handler execution in a database transaction.
    /// </summary>
    protected virtual bool UseTransaction => false;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Logger.LogInformation(
                "[{CorrelationId}] Handling {CommandType}",
                correlationId,
                typeof(TRequest).Name);

            var response = UseTransaction
                ? await UnitOfWork.ExecuteInTransactionAsync(
                    ct => HandleInternal(request, ct),
                    cancellationToken)
                : await HandleInternal(request, cancellationToken);

            stopwatch.Stop();
            Logger.LogInformation(
                "[{CorrelationId}] Handled {CommandType} in {ElapsedMs}ms",
                correlationId,
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex) when (ex is ArgumentException
            or UnauthorizedAccessException
            or InvalidOperationException
            or ValidationException)
        {
            stopwatch.Stop();
            Logger.LogWarning(
                ex,
                "[{CorrelationId}] {CommandType} failed after {ElapsedMs}ms",
                correlationId,
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(
                ex,
                "[{CorrelationId}] Unexpected error in {CommandType} after {ElapsedMs}ms",
                correlationId,
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Base handler for commands that return <see cref="Unit"/>.
/// </summary>
public abstract class BaseCommandHandler<TRequest> : BaseCommandHandler<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
    protected BaseCommandHandler(ILogger logger, IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
    }

    protected sealed override async Task<Unit> HandleInternal(
        TRequest request,
        CancellationToken cancellationToken)
    {
        await HandleCommandAsync(request, cancellationToken);
        return Unit.Value;
    }

    protected abstract Task HandleCommandAsync(TRequest request, CancellationToken cancellationToken);
}
