using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyPlacementCancelled;

public sealed record NotifyPlacementCancelledCommand(Guid PlacementId) : IRequest<int>;

public sealed class NotifyPlacementCancelledCommandHandler : IRequestHandler<NotifyPlacementCancelledCommand, int>
{
    public const string ReferenceType = "Placement";

    private readonly IMatchingRunRepository _runs;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyPlacementCancelledCommandHandler> _logger;

    public NotifyPlacementCancelledCommandHandler(
        IMatchingRunRepository runs,
        IMediator mediator,
        ILogger<NotifyPlacementCancelledCommandHandler> logger)
    {
        _runs = runs;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyPlacementCancelledCommand request, CancellationToken cancellationToken)
    {
        var placement = await _runs.FindPlacementByIdAsync(request.PlacementId, cancellationToken)
            ?? throw new KeyNotFoundException($"Placement {request.PlacementId} was not found.");

        var title = placement.Project?.Title ?? "a project";
        var ownerId = placement.Project?.CreatedByUserId ?? Guid.Empty;

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            [placement.StudentUserId, ownerId],
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Placements.PlacementCancelledTitle(),
                NotificationMessages.Placements.PlacementCancelledMessage(title),
                NotificationType.PlacementCancelled,
                userId == ownerId
                    ? NotificationLinks.FacultyProject(ownerId, placement.ProjectId)
                    : NotificationLinks.Project(placement.ProjectId),
                NotificationPriority.Critical,
                placement.Id,
                ReferenceType),
            cancellationToken);
    }
}
