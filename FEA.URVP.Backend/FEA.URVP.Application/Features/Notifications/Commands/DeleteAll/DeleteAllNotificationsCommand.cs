using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.DeleteAll;

public sealed record DeleteAllNotificationsCommand(Guid UserId) : IRequest<int>;
