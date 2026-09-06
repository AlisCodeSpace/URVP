using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Events.Semesters;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.SetActive;

public sealed class SetSemesterActiveCommandHandler
    : BaseCommandHandler<SetSemesterActiveCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;
    private readonly IEventBus _eventBus;

    public SetSemesterActiveCommandHandler(
        ILogger<SetSemesterActiveCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
        _eventBus = eventBus;
    }

    protected override async Task<SemesterDto> HandleInternal(
        SetSemesterActiveCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        var now = DateTime.UtcNow;

        if (request.IsActive)
        {
            await _semesters.RelinquishAllExceptAsync(request.Id, now, cancellationToken);
            semester.StartCycleNow(now);
        }
        else
        {
            semester.EndCycleNow(now);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Semester {SemesterId} ({Name}) cycle {Action} at {At:o}",
            semester.Id,
            semester.Name,
            request.IsActive ? "started" : "ended",
            now);

        if (request.IsActive)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new SemesterCycleStartedEvent(semester.Id),
                Logger,
                cancellationToken);
        }

        return semester.ToDto();
    }
}
