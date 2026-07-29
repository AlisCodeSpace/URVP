using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAsRead;

public class MarkAsReadCommand : IRequest<bool>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}

