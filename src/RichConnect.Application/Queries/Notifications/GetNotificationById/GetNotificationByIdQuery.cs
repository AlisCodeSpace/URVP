using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationById;

public class GetNotificationByIdQuery : IRequest<Notification?>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}

