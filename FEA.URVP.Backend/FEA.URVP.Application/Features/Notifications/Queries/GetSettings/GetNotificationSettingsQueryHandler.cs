using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetSettings;

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
