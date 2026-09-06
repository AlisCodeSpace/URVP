using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.Delete;

public sealed record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
