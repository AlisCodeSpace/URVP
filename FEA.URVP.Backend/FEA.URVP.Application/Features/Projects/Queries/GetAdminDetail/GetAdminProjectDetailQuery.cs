using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.GetAdminDetail;

public sealed class GetAdminProjectDetailQuery : IRequest<AdminProjectDetailDto>
{
    public Guid ProjectId { get; }

    public GetAdminProjectDetailQuery(Guid projectId)
    {
        ProjectId = projectId;
    }
}
