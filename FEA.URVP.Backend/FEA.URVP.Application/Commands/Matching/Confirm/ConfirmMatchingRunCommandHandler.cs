using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Matching;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.Confirm;

public sealed class ConfirmMatchingRunCommandHandler
    : BaseCommandHandler<ConfirmMatchingRunCommand, MatchingRunDetailDto>
{
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRepository _projects;

    public ConfirmMatchingRunCommandHandler(
        ILogger<ConfirmMatchingRunCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMatchingRunRepository runs,
        IProjectRepository projects)
        : base(logger, unitOfWork)
    {
        _runs = runs;
        _projects = projects;
    }

    protected override bool UseTransaction => true;

    protected override async Task<MatchingRunDetailDto> HandleInternal(
        ConfirmMatchingRunCommand request,
        CancellationToken cancellationToken)
    {
        var run = await _runs.FindByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException($"Matching run {request.RunId} was not found.");

        run.Confirm(request.CurrentUserId, DateTime.UtcNow);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        await ProjectSeatSync.ApplyAsync(
            run.Placements.Select(p => p.ProjectId),
            _projects,
            _runs,
            cancellationToken);

        Logger.LogInformation(
            "Matching run {RunId} confirmed by {UserId}: {Count} placements",
            run.Id, request.CurrentUserId, run.Placements.Count);

        return run.ToDetailDto();
    }
}
