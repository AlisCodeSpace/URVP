using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.List;

public sealed class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, (IReadOnlyList<ProjectDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;

    public ListProjectsQueryHandler(IProjectRepository projects)
    {
        _projects = projects;
    }

    public async Task<(IReadOnlyList<ProjectDto> Items, int TotalCount)> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _projects.ListAsync(
            request.CreatedByUserId,
            request.Status,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(p => p.ToDto()).ToList(), totalCount);
    }
}
