using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.DeleteNotification;

public class DeleteNotificationCommand : IRequest<bool>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}

