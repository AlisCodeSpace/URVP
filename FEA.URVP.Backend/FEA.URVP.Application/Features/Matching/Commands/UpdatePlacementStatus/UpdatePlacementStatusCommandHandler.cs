using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Matching;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events;
using FEA.URVP.Domain.Events.Matching;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;

public sealed class UpdatePlacementStatusCommandHandler
    : BaseCommandHandler<UpdatePlacementStatusCommand, PlacementDto>
{
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRepository _projects;
    private readonly IEventBus _eventBus;

    public UpdatePlacementStatusCommandHandler(
        ILogger<UpdatePlacementStatusCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMatchingRunRepository runs,
        IProjectRepository projects,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _runs = runs;
        _projects = projects;
        _eventBus = eventBus;
    }

    protected override async Task<PlacementDto> HandleInternal(
        UpdatePlacementStatusCommand request,
        CancellationToken cancellationToken)
    {
        var outcome = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        if (outcome.Event is not null)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                outcome.Event,
                Logger,
                cancellationToken);
        }

        return outcome.Dto;
    }

    private async Task<StatusOutcome> PersistAsync(
        UpdatePlacementStatusCommand request,
        CancellationToken cancellationToken)
    {
        var placement = await _runs.FindPlacementByIdAsync(request.PlacementId, cancellationToken)
            ?? throw new KeyNotFoundException($"Placement {request.PlacementId} was not found.");

        if (placement.Status != PlacementStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed placements can be declined or withdrawn.");
        }

        placement.SetStatus(request.Status, DateTime.UtcNow);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        await ProjectSeatSync.ApplyAsync([placement.ProjectId], _projects, _runs, cancellationToken);

        Logger.LogInformation(
            "Placement {PlacementId} set to {Status}; seat released on project {ProjectId}",
            placement.Id, request.Status, placement.ProjectId);

        IDomainEvent? domainEvent = request.Status switch
        {
            PlacementStatus.Declined => new PlacementDeclinedEvent(placement.Id),
            PlacementStatus.Cancelled => new PlacementCancelledEvent(placement.Id),
            _ => null,
        };

        return new StatusOutcome(placement.ToDto(), domainEvent);
    }

    private sealed record StatusOutcome(PlacementDto Dto, IDomainEvent? Event);
}
