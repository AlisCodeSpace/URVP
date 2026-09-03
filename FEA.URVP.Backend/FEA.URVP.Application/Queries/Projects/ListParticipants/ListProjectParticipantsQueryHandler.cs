using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListParticipants;

public sealed class ListProjectParticipantsQueryHandler
    : IRequestHandler<ListProjectParticipantsQuery, IReadOnlyList<ProjectParticipantDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IMatchingRunRepository _runs;

    public ListProjectParticipantsQueryHandler(
        IProjectRepository projects,
        IMatchingRunRepository runs)
    {
        _projects = projects;
        _runs = runs;
    }

    public async Task<IReadOnlyList<ProjectParticipantDto>> Handle(
        ListProjectParticipantsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (!request.IsAdmin && project.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only view participants for your own projects.");
        }

        var placements = await _runs.ListConfirmedByProjectAsync(project.Id, cancellationToken);

        return placements
            .Select(p => new ProjectParticipantDto
            {
                StudentUserId = p.StudentUserId,
                StudentName = p.StudentUser.Name,
                StudentEmail = p.StudentUser.Email,
                StudentRank = p.StudentRank,
                FacultyRank = p.FacultyRank,
            })
            .ToList();
    }
}
