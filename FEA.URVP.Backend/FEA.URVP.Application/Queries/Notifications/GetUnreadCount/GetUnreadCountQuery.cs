using MediatR;

namespace FEA.URVP.Application.Queries.Notifications.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
