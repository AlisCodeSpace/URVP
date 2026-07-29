using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetUserNotifications;

public class GetUserNotificationsQuery : IRequest<GetUserNotificationsResult>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsRead { get; set; }
}

public class GetUserNotificationsResult
{
    public List<Notification> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

