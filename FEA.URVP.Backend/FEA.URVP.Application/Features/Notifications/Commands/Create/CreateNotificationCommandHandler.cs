using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.Create;

public sealed class CreateNotificationCommandHandler
    : BaseCommandHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _notifications;
    private readonly NotificationValidationService _validation;
    private readonly IEventBus _eventBus;

    public CreateNotificationCommandHandler(
        ILogger<CreateNotificationCommandHandler> logger,
        IUnitOfWork unitOfWork,
        INotificationRepository notifications,
        NotificationValidationService validation,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _notifications = notifications;
        _validation = validation;
        _eventBus = eventBus;
    }

    protected override async Task<Guid> HandleInternal(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        await _validation.ValidateNotificationLimitAsync(request.UserId, cancellationToken);

        var outcome = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        if (outcome.Publish)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new NotificationCreatedEvent(
                    outcome.NotificationId,
                    request.UserId,
                    request.Title.Trim(),
                    request.Message.Trim(),
                    request.Type.ToString(),
                    request.Link,
                    outcome.Priority),
                Logger,
                cancellationToken);
        }

        return outcome.NotificationId;
    }

    private async Task<CreateOutcome> PersistAsync(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ReferenceId is { } referenceId)
        {
            var existing = await _notifications.FindByReferenceAsync(
                request.UserId,
                request.Type.ToString(),
                referenceId,
                cancellationToken);

            if (existing is not null)
            {
                Logger.LogInformation(
                    "Deduped notification {NotificationId} for user {UserId} type {Type} reference {ReferenceId}",
                    existing.Id,
                    request.UserId,
                    request.Type,
                    referenceId);

                return new CreateOutcome(existing.Id, existing.Priority, Publish: false);
            }
        }

        var priority = (request.Priority ?? _validation.DetermineNotificationPriority(request.Type))
            .ToStorageValue();

        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type.ToString(),
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Data = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim(),
            ReferenceId = request.ReferenceId,
            ReferenceType = string.IsNullOrWhiteSpace(request.ReferenceType)
                ? null
                : request.ReferenceType.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            Priority = priority,
        };

        _notifications.Create(notification);

        return new CreateOutcome(notification.Id, notification.Priority, Publish: true);
    }

    private sealed record CreateOutcome(Guid NotificationId, string Priority, bool Publish);
}
