using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.GetAdminDetail;

public sealed class GetAdminProjectDetailQuery : IRequest<AdminProjectDetailDto>
{
    public Guid ProjectId { get; }

    public GetAdminProjectDetailQuery(Guid projectId)
    {
        ProjectId = projectId;
    }
}
