using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAllAsRead;

public class MarkAllAsReadCommand : IRequest<int>
{
    public Guid UserId { get; set; }
}

