using FEA.URVP.Application.Abstractions.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Infrastructure.Notifications;

public sealed class LoggingPushNotificationService : IPushNotificationService
{
    private readonly ILogger<LoggingPushNotificationService> _logger;

    public LoggingPushNotificationService(ILogger<LoggingPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task QueuePushNotificationAsync(
        Guid userId,
        string title,
        string message,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Push stub queued for {UserId}: {Title} ({Data})",
            userId,
            title,
            data);
        return Task.CompletedTask;
    }

    public Task SendPushNotificationAsync(
        Guid userId,
        string title,
        string message,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Push stub send for {UserId}: {Title}", userId, title);
        return Task.CompletedTask;
    }

    public Task RegisterDeviceTokenAsync(
        Guid userId,
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Push stub register token for {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task UnregisterDeviceTokenAsync(
        Guid userId,
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Push stub unregister token for {UserId}", userId);
        return Task.CompletedTask;
    }
}
