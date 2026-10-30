using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
