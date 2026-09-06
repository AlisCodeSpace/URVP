using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Events;
using FEA.URVP.Domain.Events.Semesters;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.SetApplicationWindow;

public sealed class SetApplicationWindowCommandHandler
    : BaseCommandHandler<SetApplicationWindowCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;
    private readonly IEventBus _eventBus;

    public SetApplicationWindowCommandHandler(
        ILogger<SetApplicationWindowCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
        _eventBus = eventBus;
    }

    protected override async Task<SemesterDto> HandleInternal(
        SetApplicationWindowCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        var now = DateTime.UtcNow;
        var wasOpen = semester.IsApplicationWindowOpen(now);
        var start = request.ApplicationWindowStart;
        var end = request.ApplicationWindowEnd;

        SemesterSchedule.EnsureRange("Application window", start, end);
        SemesterSchedule.EnsureWindowWithinCycle(
            semester.CycleStart, semester.CycleEnd, start, end);

        if (!semester.IsCycleActive(now)
            && start.HasValue
            && Semester.IsWithin(start, end, now))
        {
            throw new InvalidOperationException(
                "Start the academic cycle before opening applications.");
        }

        semester.ApplyApplicationWindow(start, end, now);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Updated application window for semester {SemesterId}: {Start} → {End}",
            semester.Id,
            semester.ApplicationWindowStart?.ToString("O") ?? "—",
            semester.ApplicationWindowEnd?.ToString("O") ?? "open");

        var isOpen = semester.IsApplicationWindowOpen(now);
        IDomainEvent? domainEvent = (!wasOpen && isOpen)
            ? new ApplicationWindowOpenedEvent(semester.Id)
            : (wasOpen && !isOpen)
                ? new ApplicationWindowClosedEvent(semester.Id)
                : null;

        if (domainEvent is not null)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                domainEvent,
                Logger,
                cancellationToken);
        }

        return semester.ToDto();
    }
}
