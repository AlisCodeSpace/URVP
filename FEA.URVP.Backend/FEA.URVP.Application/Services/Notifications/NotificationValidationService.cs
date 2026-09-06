using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Services.Notifications;

public sealed class NotificationValidationService
{
    private readonly NotificationBusinessRulesService _rules;

    public NotificationValidationService(NotificationBusinessRulesService rules)
    {
        _rules = rules;
    }

    public Task<bool> ValidateUserCanReceiveNotificationAsync(
        Guid userId,
        string channel,
        CancellationToken cancellationToken = default) =>
        _rules.ValidateUserCanReceiveNotificationAsync(userId, channel, cancellationToken);

    public Task<bool> ValidateNotificationLimitAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _rules.ValidateNotificationLimitAsync(userId, cancellationToken);

    public Task<bool> ShouldSendEmailNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _rules.ShouldSendEmailNotificationAsync(userId, cancellationToken);

    public Task<bool> ShouldSendPushNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _rules.ShouldSendPushNotificationAsync(userId, cancellationToken);

    public Task<bool> CanReceivePushAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _rules.ValidateUserCanReceiveNotificationAsync(userId, NotificationChannels.Push, cancellationToken);

    public void ValidateNotificationAccess(Notification notification, Guid currentUserId) =>
        _rules.ValidateNotificationAccess(notification, currentUserId);

    public NotificationPriority DetermineNotificationPriority(NotificationType type) =>
        _rules.DetermineNotificationPriority(type);
}
