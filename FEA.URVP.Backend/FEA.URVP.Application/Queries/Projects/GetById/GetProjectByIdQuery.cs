using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetById;

public sealed class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public Guid ProjectId { get; }

    public GetProjectByIdQuery(Guid projectId)
    {
        ProjectId = projectId;
    }
}
