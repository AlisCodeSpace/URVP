using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.SetActive;

public sealed class SetSemesterActiveCommandHandler
    : BaseCommandHandler<SetSemesterActiveCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public SetSemesterActiveCommandHandler(
        ILogger<SetSemesterActiveCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task<SemesterDto> HandleInternal(
        SetSemesterActiveCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        if (request.IsActive)
        {
            // Deactivate all others before activating this one.
            await _semesters.DeactivateAllExceptAsync(request.Id, cancellationToken);
        }

        semester.IsActive = request.IsActive;
        semester.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Semester {SemesterId} ({Name}) set IsActive={IsActive}",
            semester.Id, semester.Name, semester.IsActive);

        return semester.ToDto();
    }
}
