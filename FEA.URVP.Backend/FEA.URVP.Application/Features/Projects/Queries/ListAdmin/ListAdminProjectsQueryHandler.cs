using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListAdmin;

public sealed class ListAdminProjectsQueryHandler
    : IRequestHandler<ListAdminProjectsQuery, (IReadOnlyList<AdminProjectListItemDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _rankings;
    private readonly ProjectCycleClosure _cycleClosure;

    public ListAdminProjectsQueryHandler(
        IProjectRepository projects,
        IProjectRankingRepository rankings,
        ProjectCycleClosure cycleClosure)
    {
        _projects = projects;
        _rankings = rankings;
        _cycleClosure = cycleClosure;
    }

    public async Task<(IReadOnlyList<AdminProjectListItemDto> Items, int TotalCount)> Handle(
        ListAdminProjectsQuery request,
        CancellationToken cancellationToken)
    {
        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var (items, totalCount) = await _projects.ListForAdminAsync(
            request.Search,
            request.Status,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var counts = await _rankings.CountByProjectIdsAsync(
            items.Select(p => p.Id).ToList(),
            cancellationToken);

        return (
            items.Select(p => p.ToAdminListItem(counts.GetValueOrDefault(p.Id))).ToList(),
            totalCount);
    }
}
