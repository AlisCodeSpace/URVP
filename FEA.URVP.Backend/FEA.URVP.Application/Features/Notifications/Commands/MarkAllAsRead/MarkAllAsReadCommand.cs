using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest<int>;
