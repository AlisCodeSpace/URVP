using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Projects.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListAlumni;

public sealed class ListProjectAlumniQueryHandler
    : IRequestHandler<ListProjectAlumniQuery, IReadOnlyList<ProjectAlumniDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectAlumniRepository _alumni;
    private readonly ProjectResearchAssistantAccess _access;
    private readonly ProjectCycleClosure _cycleClosure;

    public ListProjectAlumniQueryHandler(
        IProjectRepository projects,
        IProjectAlumniRepository alumni,
        ProjectCycleClosure cycleClosure,
        ProjectResearchAssistantAccess access)
    {
        _projects = projects;
        _alumni = alumni;
        _cycleClosure = cycleClosure;
        _access = access;
    }

    public async Task<IReadOnlyList<ProjectAlumniDto>> Handle(
        ListProjectAlumniQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        await _access.EnsureCanReviewAsync(
            project,
            request.CurrentUserId,
            request.IsAdmin,
            "You can only view previous students for a project you posted or assist.",
            cancellationToken);

        var rows = await _alumni.ListByProjectAsync(project.Id, cancellationToken);
        return rows
            .Select(row => new ProjectAlumniDto
            {
                SemesterId = row.SemesterId,
                SemesterName = row.SemesterName,
                StudentUserId = row.StudentUserId,
                StudentName = row.StudentName,
                StudentEmail = row.StudentEmail,
                StudentRank = row.StudentRank,
                FacultyRank = row.FacultyRank,
                WasConfirmed = row.WasConfirmed,
                ArchivedAt = row.ArchivedAt
            })
            .ToList();
    }
}
