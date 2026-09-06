using FEA.URVP.Application.Abstractions.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Infrastructure.Notifications;

/// <summary>
/// Hangfire is not in this repository. This hosted service is the only outbox processor
/// (cron equivalent: */2 * * * *).
/// </summary>
public sealed class NotificationOutboxProcessor : BackgroundService
{
    public const string JobName = "notification-outbox-processor";
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationOutboxProcessor> _logger;

    public NotificationOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting {JobName}; interval {Interval}",
            JobName,
            Interval);

        try
        {
            using var timer = new PeriodicTimer(Interval);

            await ProcessOnceAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping. Do not rethrow — StopHost treats unhandled
            // BackgroundService exceptions as fatal.
        }
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var outbox = scope.ServiceProvider.GetRequiredService<INotificationOutboxService>();
            await outbox.ProcessOutboxAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{JobName} tick failed", JobName);
        }
    }
}
