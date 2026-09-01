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

        if (semester.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot delete the active semester. Deactivate it first.");
        }

        _semesters.Remove(semester);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted semester {SemesterId} ({Name})", semester.Id, semester.Name);
    }
}
