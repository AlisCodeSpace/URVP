using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.SetRegistrationWindow;

public sealed class SetRegistrationWindowCommandHandler
    : BaseCommandHandler<SetRegistrationWindowCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public SetRegistrationWindowCommandHandler(
        ILogger<SetRegistrationWindowCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task<SemesterDto> HandleInternal(
        SetRegistrationWindowCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        var now = DateTime.UtcNow;
        SemesterSchedule.EnsureNotEnded(semester, now);

        var start = request.RegistrationWindowStart;
        var end = request.RegistrationWindowEnd;

        SemesterSchedule.EnsureRange("Registration window", start, end);
        SemesterSchedule.EnsureWindowWithinCycle(
            "The registration window", semester.CycleStart, semester.CycleEnd, start, end);
        SemesterSchedule.EnsureWindowsDoNotOverlap(
            semester.ApplicationWindowStart, semester.ApplicationWindowEnd, start, end);
        SemesterSchedule.EnsureCycleActiveToOpen(semester, start, end, now, "registration");

        semester.ApplyRegistrationWindow(start, end, now);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Updated registration window for semester {SemesterId}: {Start} → {End}",
            semester.Id,
            semester.RegistrationWindowStart?.ToString("O") ?? "—",
            semester.RegistrationWindowEnd?.ToString("O") ?? "open");

        return semester.ToDto();
    }
}
