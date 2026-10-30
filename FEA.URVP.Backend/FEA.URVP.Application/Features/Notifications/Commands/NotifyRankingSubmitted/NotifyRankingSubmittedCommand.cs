using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyRankingSubmitted;

public sealed record NotifyRankingSubmittedCommand(
    Guid RankingId,
    Guid ProjectId,
    Guid OwnerUserId,
    string ProjectTitle,
    string StudentName) : IRequest<int>;

public sealed class NotifyRankingSubmittedCommandHandler : IRequestHandler<NotifyRankingSubmittedCommand, int>
{
    public const string ReferenceType = "ProjectRanking";

    private readonly IMediator _mediator;
    private readonly ILogger<NotifyRankingSubmittedCommandHandler> _logger;

    public NotifyRankingSubmittedCommandHandler(
        IMediator mediator,
        ILogger<NotifyRankingSubmittedCommandHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task<int> Handle(NotifyRankingSubmittedCommand request, CancellationToken cancellationToken) =>
        NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            [request.OwnerUserId],
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Rankings.RankingSubmittedTitle(),
                NotificationMessages.Rankings.RankingSubmittedMessage(request.StudentName, request.ProjectTitle),
                NotificationType.RankingSubmitted,
                NotificationLinks.FacultyProject(request.OwnerUserId, request.ProjectId),
                NotificationPriority.Low,
                request.RankingId,
                ReferenceType),
            cancellationToken);
}
