using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyRankingRemoved;

public sealed record NotifyRankingRemovedCommand(
    Guid ProjectId,
    Guid OwnerUserId,
    string ProjectTitle,
    string StudentName) : IRequest<int>;

public sealed class NotifyRankingRemovedCommandHandler : IRequestHandler<NotifyRankingRemovedCommand, int>
{
    public const string ReferenceType = "Project";

    private readonly IMediator _mediator;
    private readonly ILogger<NotifyRankingRemovedCommandHandler> _logger;

    public NotifyRankingRemovedCommandHandler(
        IMediator mediator,
        ILogger<NotifyRankingRemovedCommandHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task<int> Handle(NotifyRankingRemovedCommand request, CancellationToken cancellationToken) =>
        NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            [request.OwnerUserId],
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Rankings.RankingRemovedTitle(),
                NotificationMessages.Rankings.RankingRemovedMessage(request.StudentName, request.ProjectTitle),
                NotificationType.RankingRemoved,
                NotificationLinks.FacultyProject(request.OwnerUserId, request.ProjectId),
                NotificationPriority.Low,
                request.ProjectId,
                ReferenceType),
            cancellationToken);
}
