using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetById;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly FacultyProjectMutationAccess _mutationAccess;

    public GetProjectByIdQueryHandler(
        IProjectRepository projects,
        FacultyProjectMutationAccess mutationAccess)
    {
        _projects = projects;
        _mutationAccess = mutationAccess;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var lockReason = await _mutationAccess.GetEditLockReasonAsync(project, cancellationToken);
        return project.ToDto(lockReason);
    }
}
