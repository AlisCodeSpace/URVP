using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Matching;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Events.Matching;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.Confirm;

public sealed class ConfirmMatchingRunCommandHandler
    : BaseCommandHandler<ConfirmMatchingRunCommand, MatchingRunDetailDto>
{
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRepository _projects;
    private readonly IEventBus _eventBus;

    public ConfirmMatchingRunCommandHandler(
        ILogger<ConfirmMatchingRunCommandHandler> logger,
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

    protected override async Task<MatchingRunDetailDto> HandleInternal(
        ConfirmMatchingRunCommand request,
        CancellationToken cancellationToken)
    {
        var outcome = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        await NotificationEventPublish.TryPublishAsync(
            _eventBus,
            outcome.Event,
            Logger,
            cancellationToken);

        return outcome.Dto;
    }

    private async Task<ConfirmOutcome> PersistAsync(
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

        var confirmedEvent = new MatchingRunConfirmedEvent(
            run.Id,
            request.CurrentUserId,
            run.Placements
                .Select(p => new MatchingRunConfirmedPlacement(
                    p.Id,
                    p.StudentUserId,
                    p.ProjectId,
                    p.Project.Title))
                .ToList());

        return new ConfirmOutcome(run.ToDetailDto(), confirmedEvent);
    }

    private sealed record ConfirmOutcome(MatchingRunDetailDto Dto, MatchingRunConfirmedEvent Event);
}
