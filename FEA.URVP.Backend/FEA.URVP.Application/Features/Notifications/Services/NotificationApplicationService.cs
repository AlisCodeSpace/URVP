using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Features.Notifications.Commands.Create;
using FEA.URVP.Application.Features.Notifications.Commands.Delete;
using FEA.URVP.Application.Features.Notifications.Commands.MarkAllAsRead;
using FEA.URVP.Application.Features.Notifications.Commands.MarkAsRead;
using FEA.URVP.Application.Features.Notifications.Commands.UpdateSettings;
using FEA.URVP.Application.Features.Notifications.DTOs;
using FEA.URVP.Application.Features.Notifications.Queries.GetById;
using FEA.URVP.Application.Features.Notifications.Queries.GetSettings;
using FEA.URVP.Application.Features.Notifications.Queries.GetUnreadCount;
using FEA.URVP.Application.Features.Notifications.Queries.GetUserNotifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Features.Notifications.Services;

public sealed class NotificationApplicationService : INotificationApplicationService
{
    private readonly IMediator _mediator;
    private readonly NotificationValidationService _validation;
    private readonly ILogger<NotificationApplicationService> _logger;

    public NotificationApplicationService(
        IMediator mediator,
        NotificationValidationService validation,
        ILogger<NotificationApplicationService> logger)
    {
        _mediator = mediator;
        _validation = validation;
        _logger = logger;
    }

    public async Task<Guid?> CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link = null,
        NotificationPriority? priority = null,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        var canReceivePush = await _validation.CanReceivePushAsync(userId, cancellationToken);
        if (!canReceivePush)
        {
            _logger.LogInformation(
                "Skipping in-app notification for user {UserId}; push/in-app channel is disabled.",
                userId);
            return null;
        }

        return await _mediator.Send(
            new CreateNotificationCommand(
                userId,
                title,
                message,
                type,
                link,
                priority,
                referenceId,
                referenceType),
            cancellationToken);
    }

    public Task<(IReadOnlyList<NotificationDto> Items, int TotalCount)> GetUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        bool? isRead = null,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new GetUserNotificationsQuery(userId, pageNumber, pageSize, isRead),
            cancellationToken);

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetUnreadCountQuery(userId), cancellationToken);

    public Task<NotificationDto> GetByIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetNotificationByIdQuery(notificationId, userId), cancellationToken);

    public Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new MarkAsReadCommand(notificationId, userId), cancellationToken);

    public Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _mediator.Send(new MarkAllAsReadCommand(userId), cancellationToken);

    public Task<bool> DeleteAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new DeleteNotificationCommand(notificationId, userId), cancellationToken);

    public Task<UserNotificationSettingsDto?> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new GetNotificationSettingsQuery(userId), cancellationToken);

    public Task<UserNotificationSettingsDto> UpdateSettingsAsync(
        Guid userId,
        bool emailNotifications,
        bool inAppNotifications,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new UpdateNotificationSettingsCommand(userId, emailNotifications, inAppNotifications),
            cancellationToken);
}
