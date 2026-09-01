using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.SetApplicationWindow;

public sealed class SetApplicationWindowCommandHandler
    : BaseCommandHandler<SetApplicationWindowCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public SetApplicationWindowCommandHandler(
        ILogger<SetApplicationWindowCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task<SemesterDto> HandleInternal(
        SetApplicationWindowCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        if (request.ApplicationWindowStart.HasValue
            && request.ApplicationWindowEnd.HasValue
            && request.ApplicationWindowEnd.Value <= request.ApplicationWindowStart.Value)
        {
            throw new ArgumentException(
                "Application window end must be after the start date.");
        }

        semester.ApplicationWindowStart = request.ApplicationWindowStart;
        semester.ApplicationWindowEnd = request.ApplicationWindowEnd;
        semester.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Updated application window for semester {SemesterId}: {Start} → {End}",
            semester.Id,
            semester.ApplicationWindowStart?.ToString("O") ?? "—",
            semester.ApplicationWindowEnd?.ToString("O") ?? "open");

        return semester.ToDto();
    }
}
