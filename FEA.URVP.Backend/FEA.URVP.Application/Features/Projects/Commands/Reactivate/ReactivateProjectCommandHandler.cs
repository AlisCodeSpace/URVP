using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Commands.Semesters;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Projects.Reactivate;

public sealed class ReactivateProjectCommandHandler
    : BaseCommandHandler<ReactivateProjectCommand, ProjectDto>
{
    public const string NotInactiveMessage =
        "Only an inactive project from a previous cycle can be reactivated.";

    public const string AlreadyCurrentMessage =
        "This project already belongs to the current academic cycle.";

    private readonly IProjectRepository _projects;
    private readonly ISemesterRepository _semesters;
    private readonly ProjectCycleClosure _cycleClosure;

    public ReactivateProjectCommandHandler(
        ILogger<ReactivateProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        ISemesterRepository semesters,
        ProjectCycleClosure cycleClosure)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _semesters = semesters;
        _cycleClosure = cycleClosure;
    }

    protected override async Task<ProjectDto> HandleInternal(
        ReactivateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (!request.IsAdmin && project.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only reactivate your own projects.");
        }

        var now = DateTime.UtcNow;
        var semester = await _semesters.FindActiveAsync(cancellationToken);
        ApplicationWindowRules.EnsureRegistrationOpen(
            semester,
            now,
            "Projects can be reactivated only while the registration window is open.");

        if (project.SemesterId == semester!.Id)
        {
            throw new InvalidOperationException(AlreadyCurrentMessage);
        }

        if (project.Status != ProjectStatus.Inactive)
        {
            throw new InvalidOperationException(NotInactiveMessage);
        }

        project.SemesterId = semester.Id;
        project.Semester = semester;
        project.Status = ProjectStatus.Open;
        project.VolunteersFilled = 0;
        project.UpdatedAt = now;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Reactivated project {ProjectId} onto semester {SemesterId} by user {UserId}",
            project.Id,
            semester.Id,
            request.CurrentUserId);

        return project.ToDto();
    }
}
