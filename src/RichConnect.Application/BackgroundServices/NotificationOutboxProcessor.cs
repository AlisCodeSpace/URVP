using Microsoft.Extensions.Configuration;
using RICHConnect.Backend.Application.Services.Notifications;

namespace RICHConnect.Backend.Application.BackgroundServices
{
    /// <summary>
    /// Background service that processes notification outbox items
    /// </summary>
    public class NotificationOutboxProcessor : BackgroundService
    {
        private const int DefaultIntervalSeconds = 60;
        private const int MinIntervalSeconds = 30;
        private const int MaxIntervalSeconds = 600;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationOutboxProcessor> _logger;
        private readonly TimeSpan _processingInterval;

        public NotificationOutboxProcessor(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<NotificationOutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var configuredSeconds = configuration.GetValue("NotificationOutbox:ProcessingIntervalSeconds", DefaultIntervalSeconds);
            var clampedSeconds = Math.Clamp(configuredSeconds, MinIntervalSeconds, MaxIntervalSeconds);
            if (configuredSeconds != clampedSeconds)
            {
                _logger.LogWarning(
                    "NotificationOutbox:ProcessingIntervalSeconds {Value} is outside allowed range [{Min}, {Max}], using {Clamped}",
                    configuredSeconds, MinIntervalSeconds, MaxIntervalSeconds, clampedSeconds);
            }
            _processingInterval = TimeSpan.FromSeconds(clampedSeconds);
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationOutboxProcessor started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxAsync(stoppingToken);
                    
                    // Wait before next processing cycle
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationOutboxProcessor");
                    
                    // Wait a bit before retrying after error
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            
            _logger.LogInformation("NotificationOutboxProcessor stopped");
        }
        
        private async Task ProcessOutboxAsync(CancellationToken stoppingToken)
        {
            // Create a new scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            
            try
            {
                var outboxService = scope.ServiceProvider.GetRequiredService<NotificationOutboxService>();
                await outboxService.ProcessOutboxAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification outbox");
            }
        }
    }
}
