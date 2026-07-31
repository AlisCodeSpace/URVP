using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.List;

public sealed class ListProjectsQuery : IRequest<(IReadOnlyList<ProjectDto> Items, int TotalCount)>
{
    public Guid? CreatedByUserId { get; }
    public ProjectStatus? Status { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListProjectsQuery(
        Guid? createdByUserId,
        ProjectStatus? status,
        int pageNumber,
        int pageSize)
    {
        CreatedByUserId = createdByUserId;
        Status = status;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
