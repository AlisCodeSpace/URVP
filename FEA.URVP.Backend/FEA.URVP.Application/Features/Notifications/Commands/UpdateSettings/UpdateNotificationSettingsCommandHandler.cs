using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.UpdateSettings;

public sealed class UpdateNotificationSettingsCommandHandler
    : BaseCommandHandler<UpdateNotificationSettingsCommand, UserNotificationSettingsDto>
{
    private readonly INotificationRepository _notifications;
    private readonly IEventBus _eventBus;

    public UpdateNotificationSettingsCommandHandler(
        ILogger<UpdateNotificationSettingsCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INotificationRepository notifications,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _notifications = notifications;
        _eventBus = eventBus;
    }

    protected override async Task<UserNotificationSettingsDto> HandleInternal(
        UpdateNotificationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        await NotificationEventPublish.TryPublishAsync(
            _eventBus,
            new NotificationSettingsUpdatedEvent(
                settings.UserId,
                settings.EmailNotifications,
                settings.InAppNotifications),
            Logger,
            cancellationToken);

        return settings.ToDto();
    }

    private async Task<UserNotificationSettings> PersistAsync(
        UpdateNotificationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var settings = await _notifications.GetSettingsAsync(request.UserId, cancellationToken);

        if (settings is null)
        {
            settings = new UserNotificationSettings
            {
                UserId = request.UserId,
                EmailNotifications = request.EmailNotifications,
                InAppNotifications = request.InAppNotifications,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _notifications.CreateSettings(settings);
            return settings;
        }

        settings.EmailNotifications = request.EmailNotifications;
        settings.InAppNotifications = request.InAppNotifications;
        settings.UpdatedAt = now;
        _notifications.UpdateSettings(settings);
        return settings;
    }
}
