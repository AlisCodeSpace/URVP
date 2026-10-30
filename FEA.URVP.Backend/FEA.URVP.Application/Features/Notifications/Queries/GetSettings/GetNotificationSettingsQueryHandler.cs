using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Notifications.DTOs;
using FEA.URVP.Application.Features.Notifications.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetSettings;

public sealed class GetNotificationSettingsQueryHandler
    : IRequestHandler<GetNotificationSettingsQuery, UserNotificationSettingsDto?>
{
    private readonly INotificationRepository _notifications;

    public GetNotificationSettingsQueryHandler(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public async Task<UserNotificationSettingsDto?> Handle(
        GetNotificationSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _notifications.GetSettingsAsync(request.UserId, cancellationToken);
        return settings?.ToDto();
    }
}
