using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Cached SQL connectivity probe for the readiness endpoint.
/// </summary>
/// <remarks>
/// Callers share one result for <see cref="CacheDuration"/> so a probe loop cannot open a live
/// database connection on every request.
/// </remarks>
public sealed class DatabaseReadinessCheck
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<DatabaseReadinessCheck> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Snapshot? _cached;
    private DateTimeOffset _validUntil;

    public DatabaseReadinessCheck(
        IServiceScopeFactory scopes,
        ILogger<DatabaseReadinessCheck> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    public async Task<Snapshot> GetAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (_cached is { } fresh && now < _validUntil)
        {
            return fresh;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            now = DateTimeOffset.UtcNow;
            if (_cached is { } shared && now < _validUntil)
            {
                return shared;
            }

            var snapshot = await ProbeAsync(cancellationToken);
            _cached = snapshot;
            _validUntil = DateTimeOffset.UtcNow.Add(CacheDuration);
            return snapshot;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Snapshot> ProbeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ProbeTimeout);

            var canConnect = await db.Database.CanConnectAsync(timeout.Token);
            return new Snapshot(canConnect, canConnect ? null : "ConnectionRefused");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness probe: database connectivity check failed.");
            return new Snapshot(false, ex.GetType().Name);
        }
    }

    public readonly record struct Snapshot(bool IsReachable, string? FailureType);
}
