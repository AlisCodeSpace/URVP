using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Exports;
using FEA.URVP.Application.Projects;
using FEA.URVP.Application.Queries.Users.Export;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.Export;

public sealed class ExportAdminProjectsQueryHandler
    : IRequestHandler<ExportAdminProjectsQuery, UserExportFileDto>
{
    private readonly IProjectRepository _projects;
    private readonly IMatchingRunRepository _runs;
    private readonly ProjectCycleClosure _cycleClosure;

    public ExportAdminProjectsQueryHandler(
        IProjectRepository projects,
        IMatchingRunRepository runs,
        ProjectCycleClosure cycleClosure)
    {
        _projects = projects;
        _runs = runs;
        _cycleClosure = cycleClosure;
    }

    public async Task<UserExportFileDto> Handle(
        ExportAdminProjectsQuery request,
        CancellationToken cancellationToken)
    {
        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var projects = await _projects.ListAllForAdminAsync(
            request.Search,
            request.Status,
            cancellationToken);
        var placements = await _runs.ListConfirmedByProjectIdsAsync(
            projects.Select(project => project.Id).ToArray(),
            cancellationToken);
        var rows = ProjectExportMapper.Build(projects, placements);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");

        return new UserExportFileDto
        {
            Content = ProjectExportDocuments.ToExcel(rows),
            MimeType = ExportUsersQueryHandler.ExcelMime,
            FileName = $"urvp-projects-{stamp}.xlsx"
        };
    }
}
