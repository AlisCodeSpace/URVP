using RICHConnect.Backend.Application.Services.Notifications;

namespace RICHConnect.Backend.Application.BackgroundServices
{
    /// <summary>
    /// Hangfire job wrapper that processes notification outbox items.
    /// This delegates to <see cref="NotificationOutboxService"/> so all business
    /// logic and retry behavior stays in one place.
    /// </summary>
    public class NotificationOutboxHangfireJob
    {
        private readonly NotificationOutboxService _outboxService;
        private readonly ILogger<NotificationOutboxHangfireJob> _logger;

        public NotificationOutboxHangfireJob(
            NotificationOutboxService outboxService,
            ILogger<NotificationOutboxHangfireJob> logger)
        {
            _outboxService = outboxService;
            _logger = logger;
        }

        /// <summary>
        /// Entry point invoked by Hangfire recurring job.
        /// </summary>
        public async Task ProcessOutboxAsync()
        {
            _logger.LogInformation("Hangfire notification outbox job started");

            try
            {
                await _outboxService.ProcessOutboxAsync(CancellationToken.None);
                _logger.LogInformation("Hangfire notification outbox job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hangfire notification outbox job failed");
                throw;
            }
        }
    }
}

