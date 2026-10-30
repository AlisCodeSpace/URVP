using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyMatchingConfirmed;

public sealed class NotifyMatchingConfirmedCommandHandler
    : IRequestHandler<NotifyMatchingConfirmedCommand, int>
{
    public const string ReferenceType = "Placement";

    private readonly IMatchingRunRepository _runs;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyMatchingConfirmedCommandHandler> _logger;

    public NotifyMatchingConfirmedCommandHandler(
        IMatchingRunRepository runs,
        IMediator mediator,
        ILogger<NotifyMatchingConfirmedCommandHandler> logger)
    {
        _runs = runs;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(
        NotifyMatchingConfirmedCommand request,
        CancellationToken cancellationToken)
    {
        var run = await _runs.FindByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException($"Matching run {request.RunId} was not found.");

        var confirmed = run.Placements.Where(p => p.Status == PlacementStatus.Confirmed).ToList();
        var created = await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            confirmed.Select(p => p.StudentUserId),
            userId =>
            {
                var placement = confirmed.First(p => p.StudentUserId == userId);
                return new CreateNotificationCommand(
                    UserId: userId,
                    Title: NotificationMessages.Matching.MatchingConfirmedTitle(),
                    Message: NotificationMessages.Matching.MatchingConfirmedMessage(),
                    Type: NotificationType.MatchingConfirmed,
                    Link: NotificationLinks.Project(placement.ProjectId),
                    Priority: NotificationPriority.High,
                    ReferenceId: placement.Id,
                    ReferenceType: ReferenceType);
            },
            cancellationToken);

        foreach (var projectGroup in confirmed
                     .Where(p => p.Project is not null)
                     .GroupBy(p => new { p.Project.CreatedByUserId, p.ProjectId }))
        {
            var ownerId = projectGroup.Key.CreatedByUserId;
            var projectId = projectGroup.Key.ProjectId;
            created += await NotificationFanOut.SendAsync(
                _mediator,
                _logger,
                [ownerId],
                userId => new CreateNotificationCommand(
                    UserId: userId,
                    Title: NotificationMessages.Matching.MatchingConfirmedTitle(),
                    Message: NotificationMessages.Matching.MatchingConfirmedMessage(),
                    Type: NotificationType.MatchingConfirmed,
                    Link: NotificationLinks.FacultyProject(ownerId, projectId),
                    Priority: NotificationPriority.High,
                    ReferenceId: projectId,
                    ReferenceType: "Project"),
                cancellationToken);
        }

        return created;
    }
}
