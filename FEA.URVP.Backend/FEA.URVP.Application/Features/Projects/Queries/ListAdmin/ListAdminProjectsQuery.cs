using FEA.URVP.Application.Features.Projects.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListAdmin;

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
