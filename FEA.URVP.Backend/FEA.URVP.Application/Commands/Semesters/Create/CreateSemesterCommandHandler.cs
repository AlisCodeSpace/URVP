using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.Semesters;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Semesters.Create;

public sealed class CreateSemesterCommandHandler
    : BaseCommandHandler<CreateSemesterCommand, SemesterDto>
{
    private readonly ISemesterRepository _semesters;

    public CreateSemesterCommandHandler(
        ILogger<CreateSemesterCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
    }

    protected override async Task<SemesterDto> HandleInternal(
        CreateSemesterCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var semester = new Semester
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _semesters.Add(semester);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created semester {SemesterId} ({Name})", semester.Id, semester.Name);

        return semester.ToDto();
    }
}
