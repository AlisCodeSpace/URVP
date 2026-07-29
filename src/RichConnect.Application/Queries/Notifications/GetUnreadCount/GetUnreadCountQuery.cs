using MediatR;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetUnreadCount;

public class GetUnreadCountQuery : IRequest<int>
{
    public Guid UserId { get; set; }
}

