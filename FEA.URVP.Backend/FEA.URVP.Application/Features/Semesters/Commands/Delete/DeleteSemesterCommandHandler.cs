using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.Delete;

public sealed class DeleteSemesterCommandHandler
    : BaseCommandHandler<DeleteSemesterCommand>
{
    private readonly ISemesterRepository _semesters;

    public DeleteSemesterCommandHandler(
        ILogger<DeleteSemesterCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task HandleCommandAsync(
        DeleteSemesterCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        var now = DateTime.UtcNow;
        if (semester.IsCycleActive(now))
        {
            throw new InvalidOperationException(
                "Cannot delete the active semester. End the cycle first.");
        }

        if (semester.HasEnded(now))
        {
            throw new InvalidOperationException(
                "Ended cycles are kept as history and cannot be deleted.");
        }

        _semesters.Remove(semester);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted semester {SemesterId} ({Name})", semester.Id, semester.Name);
    }
}
