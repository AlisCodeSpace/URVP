using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
