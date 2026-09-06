using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Application.Services.Notifications;

public sealed class NotificationOutboxService : INotificationOutboxService
{
    public const int BatchSize = 50;
    public const int MaxRetries = 5;

    private readonly INotificationOutboxRepository _outbox;
    private readonly INotificationRepository _notifications;
    private readonly IEmailService _email;
    private readonly IUserEmailService _userEmails;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<NotificationOutboxService> _logger;

    public NotificationOutboxService(
        INotificationOutboxRepository outbox,
        INotificationRepository notifications,
        IEmailService email,
        IUserEmailService userEmails,
        IUnitOfWork unitOfWork,
        IOptions<EmailOptions> emailOptions,
        ILogger<NotificationOutboxService> logger)
    {
        _outbox = outbox;
        _notifications = notifications;
        _email = email;
        _userEmails = userEmails;
        _unitOfWork = unitOfWork;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task QueueEmailNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _outbox.GetByNotificationIdAsync(notificationId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation(
                "Outbox already has {EventType} for notification {NotificationId}",
                existing.EventType,
                notificationId);
            return;
        }

        _outbox.Create(new NotificationOutbox
        {
            NotificationId = notificationId,
            EventType = NotificationOutboxEventTypes.EmailNotification,
            Status = nameof(NotificationOutboxStatus.Pending),
            CreatedAt = DateTime.UtcNow,
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessOutboxAsync(CancellationToken cancellationToken = default)
    {
        var items = await _outbox.GetPendingItemsAsync(BatchSize, cancellationToken);
        if (items.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} notification outbox item(s)", items.Count);

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _outbox.UpdateStatusAsync(
                    item.Id,
                    NotificationOutboxStatus.Processing,
                    cancellationToken: cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await ProcessItemAsync(item, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox item {OutboxId} failed; scheduling retry",
                    item.Id);
                Track(NotificationAnalyticsEvents.EmailSendFailed, item.NotificationId, ex.Message);
                await HandleRetryAsync(item, ex, cancellationToken);
            }
        }
    }

    private async Task ProcessItemAsync(NotificationOutbox item, CancellationToken cancellationToken)
    {
        if (!string.Equals(
                item.EventType,
                NotificationOutboxEventTypes.EmailNotification,
                StringComparison.Ordinal))
        {
            await FailAsync(item.Id, $"Unsupported EventType '{item.EventType}'.", cancellationToken);
            return;
        }

        var notification = await _notifications.GetByIdAsync(item.NotificationId, cancellationToken);
        if (notification is null)
        {
            await FailAsync(item.Id, "Notification is missing.", cancellationToken);
            return;
        }

        var email = await _userEmails.GetUserEmailAsync(notification.UserId, cancellationToken);
        if (string.IsNullOrWhiteSpace(email))
        {
            await FailAsync(item.Id, "User email is missing; skipped.", cancellationToken);
            return;
        }

        var name = await _userEmails.GetUserNameAsync(notification.UserId, cancellationToken)
            ?? email;

        string? actionUrl = null;
        string? actionText = null;
        if (NotificationMessages.RequiresSignInAction(notification.Type)
            && !string.IsNullOrWhiteSpace(_emailOptions.PortalSignInUrl))
        {
            actionUrl = _emailOptions.PortalSignInUrl;
            actionText = _emailOptions.SignInActionText;
        }

        var sent = await _email.SendEmailAsync(
            email,
            name,
            notification.Title,
            notification.Message,
            actionUrl,
            actionText,
            cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException("Email send returned false.");
        }

        await _outbox.UpdateStatusAsync(
            item.Id,
            NotificationOutboxStatus.Completed,
            cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Track(NotificationAnalyticsEvents.EmailSent, notification.Id);
        _logger.LogInformation(
            "Sent email for notification {NotificationId} via outbox {OutboxId}",
            notification.Id,
            item.Id);
    }

    private async Task HandleRetryAsync(
        NotificationOutbox item,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var nextCount = item.RetryCount + 1;
        var error = exception.Message;

        try
        {
            if (nextCount >= MaxRetries)
            {
                await _outbox.IncrementRetryAsync(item.Id, error, nextRetryAt: null, cancellationToken);
                await _outbox.UpdateStatusAsync(
                    item.Id,
                    NotificationOutboxStatus.Failed,
                    error,
                    cancellationToken);
                item.RetryCount = nextCount;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogWarning(
                    "Outbox item {OutboxId} reached {MaxRetries} retries and is Failed",
                    item.Id,
                    MaxRetries);
                return;
            }

            var delayMinutes = Math.Pow(2, nextCount);
            var nextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
            await _outbox.IncrementRetryAsync(item.Id, error, nextRetryAt, cancellationToken);
            await _outbox.UpdateStatusAsync(
                item.Id,
                NotificationOutboxStatus.Pending,
                error,
                cancellationToken);
            item.RetryCount = nextCount;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Outbox item {OutboxId} retry {RetryCount} scheduled at {NextRetryAt}",
                item.Id,
                nextCount,
                nextRetryAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist retry state for outbox item {OutboxId}", item.Id);
        }
    }

    private async Task FailAsync(Guid outboxId, string reason, CancellationToken cancellationToken)
    {
        await _outbox.UpdateStatusAsync(
            outboxId,
            NotificationOutboxStatus.Failed,
            reason,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Outbox item {OutboxId} marked Failed: {Reason}", outboxId, reason);
    }

    private void Track(string analyticsEvent, Guid notificationId, string? reason = null) =>
        _logger.LogInformation(
            "Analytics event {AnalyticsEvent} NotificationId={NotificationId} Reason={Reason}",
            analyticsEvent,
            notificationId,
            reason);
}
