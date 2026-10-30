using MediatR;

namespace FEA.URVP.Application.Features.Notifications.Commands.NotifyMatchingConfirmed;

public sealed record NotifyMatchingConfirmedCommand(Guid RunId) : IRequest<int>;
