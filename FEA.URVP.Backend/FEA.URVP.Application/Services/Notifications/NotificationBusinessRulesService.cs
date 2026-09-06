using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Application.Services.Notifications;

public sealed class NotificationBusinessRulesService
{
    private readonly INotificationRepository _notifications;
    private readonly NotificationSettingsOptions _settings;
    private readonly ILogger<NotificationBusinessRulesService> _logger;

    public NotificationBusinessRulesService(
        INotificationRepository notifications,
        IOptions<NotificationSettingsOptions> settings,
        ILogger<NotificationBusinessRulesService> logger)
    {
        _notifications = notifications;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> ValidateUserCanReceiveNotificationAsync(
        Guid userId,
        string channel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);

        return channel.Trim().ToLowerInvariant() switch
        {
            NotificationChannels.Email => await ShouldSendEmailNotificationAsync(userId, cancellationToken),
            NotificationChannels.Push => await ShouldSendPushNotificationAsync(userId, cancellationToken),
            _ => throw new ArgumentException(
                $"Unknown notification channel '{channel}'. Expected '{NotificationChannels.Email}' or '{NotificationChannels.Push}'.",
                nameof(channel)),
        };
    }

    public async Task<bool> ValidateNotificationLimitAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var unread = await _notifications.GetUnreadCountAsync(userId, cancellationToken);
        var maxUnread = _settings.MaxUnreadNotifications > 0
            ? _settings.MaxUnreadNotifications
            : 100;

        if (unread < maxUnread)
        {
            return true;
        }

        _logger.LogWarning(
            "User {UserId} has {UnreadCount} unread notifications, which meets or exceeds the advisory limit of {MaxUnread}.",
            userId,
            unread,
            maxUnread);

        return false;
    }

    public async Task<bool> ShouldSendEmailNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _notifications.GetSettingsAsync(userId, cancellationToken);
        return settings?.EmailNotifications ?? true;
    }

    public async Task<bool> ShouldSendPushNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _notifications.GetSettingsAsync(userId, cancellationToken);
        return settings?.InAppNotifications ?? true;
    }

    public void ValidateNotificationAccess(Notification notification, Guid currentUserId)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification.UserId != currentUserId)
        {
            throw new KeyNotFoundException("Notification was not found.");
        }
    }

    public NotificationPriority DetermineNotificationPriority(NotificationType type) => type switch
    {
        NotificationType.ProjectApproved => NotificationPriority.High,
        NotificationType.PlacementConfirmed => NotificationPriority.High,
        NotificationType.MatchingConfirmed => NotificationPriority.High,
        NotificationType.PlacementCancelled => NotificationPriority.Critical,
        NotificationType.ProjectDeleted => NotificationPriority.Critical,
        NotificationType.PlacementDeclined => NotificationPriority.High,
        NotificationType.ApplicationWindowOpened => NotificationPriority.Medium,
        NotificationType.ApplicationWindowClosed => NotificationPriority.Medium,
        NotificationType.SemesterCycleStarted => NotificationPriority.Medium,
        NotificationType.ProjectOpen => NotificationPriority.Medium,
        NotificationType.RoleAssigned => NotificationPriority.Medium,
        NotificationType.StudentProfileSubmitted => NotificationPriority.Low,
        NotificationType.ProjectClosed => NotificationPriority.Low,
        NotificationType.RankingSubmitted => NotificationPriority.Low,
        NotificationType.RankingRemoved => NotificationPriority.Low,
        NotificationType.FacultyRankingSubmitted => NotificationPriority.Low,
        NotificationType.NewsPublished => NotificationPriority.Low,
        NotificationType.WorkshopAnnounced => NotificationPriority.Low,
        _ => NotificationPriority.Low,
    };
}
