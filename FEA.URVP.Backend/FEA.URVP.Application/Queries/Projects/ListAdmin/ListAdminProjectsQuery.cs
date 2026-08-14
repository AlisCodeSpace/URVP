using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListAdmin;

public sealed class ListAdminProjectsQuery
    : IRequest<(IReadOnlyList<AdminProjectListItemDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public ProjectStatus? Status { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListAdminProjectsQuery(
        string? search,
        ProjectStatus? status,
        int pageNumber,
        int pageSize)
    {
        Search = search;
        Status = status;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
