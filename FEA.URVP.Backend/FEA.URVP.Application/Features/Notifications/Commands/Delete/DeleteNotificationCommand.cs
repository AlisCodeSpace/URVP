using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.Delete;

public sealed record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
