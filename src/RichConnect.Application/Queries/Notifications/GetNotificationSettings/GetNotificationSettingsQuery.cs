using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationSettings;

public class GetNotificationSettingsQuery : IRequest<UserNotificationSettings?>
{
    public Guid UserId { get; set; }
}

