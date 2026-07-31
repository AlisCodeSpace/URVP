using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetById;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projects;

    public GetProjectByIdQueryHandler(IProjectRepository projects)
    {
        _projects = projects;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        return project.ToDto();
    }
}
