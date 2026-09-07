using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyPlacementConfirmed;

public sealed record NotifyPlacementConfirmedCommand(Guid PlacementId) : IRequest<int>;

public sealed class NotifyPlacementConfirmedCommandHandler
    : IRequestHandler<NotifyPlacementConfirmedCommand, int>
{
    public const string ReferenceType = "Placement";

    private readonly IMatchingRunRepository _runs;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyPlacementConfirmedCommandHandler> _logger;

    public NotifyPlacementConfirmedCommandHandler(
        IMatchingRunRepository runs,
        IMediator mediator,
        ILogger<NotifyPlacementConfirmedCommandHandler> logger)
    {
        _runs = runs;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(
        NotifyPlacementConfirmedCommand request,
        CancellationToken cancellationToken)
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
                NotificationMessages.Placements.PlacementConfirmedTitle(),
                NotificationMessages.Placements.PlacementConfirmedMessage(title),
                NotificationType.PlacementConfirmed,
                userId == ownerId
                    ? NotificationLinks.FacultyProject(ownerId, placement.ProjectId)
                    : NotificationLinks.Project(placement.ProjectId),
                NotificationPriority.High,
                placement.Id,
                ReferenceType),
            cancellationToken);
    }
}
