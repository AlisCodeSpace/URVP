using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Matching;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;

public sealed class UpdatePlacementStatusCommandHandler
    : BaseCommandHandler<UpdatePlacementStatusCommand, PlacementDto>
{
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRepository _projects;

    public UpdatePlacementStatusCommandHandler(
        ILogger<UpdatePlacementStatusCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMatchingRunRepository runs,
        IProjectRepository projects)
        : base(logger, unitOfWork)
    {
        _runs = runs;
        _projects = projects;
    }

    protected override bool UseTransaction => true;

    protected override async Task<PlacementDto> HandleInternal(
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

        return placement.ToDto();
    }
}
