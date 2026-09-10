using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.List;

public sealed class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, (IReadOnlyList<ProjectDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;
    private readonly FacultyProjectMutationAccess _mutationAccess;

    public ListProjectsQueryHandler(
        IProjectRepository projects,
        FacultyProjectMutationAccess mutationAccess)
    {
        _projects = projects;
        _mutationAccess = mutationAccess;
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

        var lockReasons = request.CreatedByUserId.HasValue
            ? await _mutationAccess.GetEditLockReasonsAsync(items, cancellationToken)
            : null;

        var dtos = items
            .Select(project => project.ToDto(
                lockReasons?.GetValueOrDefault(project.Id)))
            .ToList();

        return (dtos, totalCount);
    }
}
