using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.MarkAllAsRead;

public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest<int>;
