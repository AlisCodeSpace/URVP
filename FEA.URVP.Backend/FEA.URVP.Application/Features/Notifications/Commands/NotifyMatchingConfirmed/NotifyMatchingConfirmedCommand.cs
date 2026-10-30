using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.NotifyMatchingConfirmed;

public sealed record NotifyMatchingConfirmedCommand(Guid RunId) : IRequest<int>;
