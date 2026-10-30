using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
