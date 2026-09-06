namespace FEA.URVP.Application.Abstractions.Notifications;

public interface IPushNotificationService
{
    Task QueuePushNotificationAsync(
        Guid userId,
        string title,
        string message,
        string? data = null,
        CancellationToken cancellationToken = default);

    Task SendPushNotificationAsync(
        Guid userId,
        string title,
        string message,
        string? data = null,
        CancellationToken cancellationToken = default);

    Task RegisterDeviceTokenAsync(
        Guid userId,
        string deviceToken,
        CancellationToken cancellationToken = default);

    Task UnregisterDeviceTokenAsync(
        Guid userId,
        string deviceToken,
        CancellationToken cancellationToken = default);
}
