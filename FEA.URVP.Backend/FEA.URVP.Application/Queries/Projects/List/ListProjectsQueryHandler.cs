using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Semesters;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.List;

public sealed class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, (IReadOnlyList<ProjectDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;
    private readonly ISemesterRepository _semesters;
    private readonly FacultyProjectMutationAccess _mutationAccess;
    private readonly ProjectCycleClosure _cycleClosure;

    public ListProjectsQueryHandler(
        IProjectRepository projects,
        ISemesterRepository semesters,
        FacultyProjectMutationAccess mutationAccess,
        ProjectCycleClosure cycleClosure)
    {
        _projects = projects;
        _semesters = semesters;
        _mutationAccess = mutationAccess;
        _cycleClosure = cycleClosure;
    }

    public async Task<(IReadOnlyList<ProjectDto> Items, int TotalCount)> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var active = await _semesters.FindActiveAsync(cancellationToken);
        var catalog = request.CreatedByUserId is null;

        if (catalog && request.ViewerIsStudent)
        {
            ApplicationWindowRules.EnsureStudentsCanViewProjects(
                active, now, viewerIsStudent: true);
        }

        if (catalog && (active is null || !active.IsCycleActive(now)))
        {
            return ([], 0);
        }

        var (items, totalCount) = await _projects.ListAsync(
            request.CreatedByUserId,
            request.Status,
            request.PageNumber,
            request.PageSize,
            catalog ? active!.Id : null,
            excludeInactive: catalog,
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
