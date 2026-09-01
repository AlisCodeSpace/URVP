using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.Update;

public sealed class UpdateSemesterCommandHandler
    : BaseCommandHandler<UpdateSemesterCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public UpdateSemesterCommandHandler(
        ILogger<UpdateSemesterCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task<SemesterDto> HandleInternal(
        UpdateSemesterCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.Id} was not found.");

        semester.Name = request.Name.Trim();
        semester.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        semester.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated semester {SemesterId} ({Name})", semester.Id, semester.Name);

        return semester.ToDto();
    }
}
