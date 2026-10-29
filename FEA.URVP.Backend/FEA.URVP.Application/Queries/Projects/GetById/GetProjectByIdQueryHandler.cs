using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Semesters;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetById;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly ISemesterRepository _semesters;
    private readonly IMatchingRunRepository _runs;
    private readonly IUserRepository _users;
    private readonly IProjectResearchAssistantRepository _assistants;
    private readonly FacultyProjectMutationAccess _mutationAccess;
    private readonly ProjectCycleClosure _cycleClosure;

    public GetProjectByIdQueryHandler(
        IProjectRepository projects,
        ISemesterRepository semesters,
        FacultyProjectMutationAccess mutationAccess,
        ProjectCycleClosure cycleClosure,
        IMatchingRunRepository runs,
        IUserRepository users,
        IProjectResearchAssistantRepository assistants)
    {
        _projects = projects;
        _semesters = semesters;
        _mutationAccess = mutationAccess;
        _cycleClosure = cycleClosure;
        _runs = runs;
        _users = users;
        _assistants = assistants;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var now = DateTime.UtcNow;
        var active = await _semesters.FindActiveAsync(cancellationToken);
        var onCurrentCycle = ProjectVisibility.IsOnCurrentCycle(project, active, now);
        var assists = await ProjectResearchAssistantAccess.IsListedAsync(
            _users,
            _assistants,
            request.ViewerUserId,
            project.Id,
            cancellationToken);
        var canSeePreviousCycle = request.ViewerIsAdmin
            || project.CreatedByUserId == request.ViewerUserId
            || assists;

        if (!onCurrentCycle && !canSeePreviousCycle)
        {
            throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");
        }

        if (request.ViewerIsStudent && !canSeePreviousCycle)
        {
            var participating = await _runs.StudentOccupiesProjectAsync(
                request.ViewerUserId,
                request.ProjectId,
                cancellationToken);
            if (!participating)
            {
                ApplicationWindowRules.EnsureStudentsCanViewProjects(
                    active, now, viewerIsStudent: true);
            }
        }

        var lockReason = await _mutationAccess.GetEditLockReasonAsync(project, cancellationToken);
        return project.ToDto(lockReason);
    }
}
