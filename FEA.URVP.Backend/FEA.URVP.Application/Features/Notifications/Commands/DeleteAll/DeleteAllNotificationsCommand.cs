using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.DeleteAll;

public sealed record DeleteAllNotificationsCommand(Guid UserId) : IRequest<int>;
