using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Notifications.DTOs;
using FEA.URVP.Application.Features.Notifications.Mappings;
using FEA.URVP.Application.Features.Notifications.Services;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetById;

public sealed class GetNotificationByIdQueryHandler
    : IRequestHandler<GetNotificationByIdQuery, NotificationDto>
{
    private readonly INotificationRepository _notifications;
    private readonly NotificationValidationService _validation;

    public GetNotificationByIdQueryHandler(
        INotificationRepository notifications,
        NotificationValidationService validation)
    {
        _notifications = notifications;
        _validation = validation;
    }

    public async Task<NotificationDto> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Notification {request.NotificationId} was not found.");

        _validation.ValidateNotificationAccess(notification, request.UserId);
        return notification.ToDto();
    }
}
