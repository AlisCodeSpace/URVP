using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.Projects.ListParticipants;

public sealed class ListProjectParticipantsQueryHandler
    : IRequestHandler<ListProjectParticipantsQuery, IReadOnlyList<ProjectParticipantDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IMatchingRunRepository _runs;
    private readonly IUserRepository _users;
    private readonly IProjectResearchAssistantRepository _assistants;

    public ListProjectParticipantsQueryHandler(
        IProjectRepository projects,
        IMatchingRunRepository runs,
        IUserRepository users,
        IProjectResearchAssistantRepository assistants)
    {
        _projects = projects;
        _runs = runs;
        _users = users;
        _assistants = assistants;
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

        await ProjectResearchAssistantAccess.EnsureCanReviewAsync(
            project,
            request.CurrentUserId,
            request.IsAdmin,
            _users,
            _assistants,
            "You can only view participants for a project you posted or assist.",
            cancellationToken);

        var placements = await _runs.ListConfirmedByProjectAsync(project.Id, cancellationToken);

        return placements
            .Select(p => new ProjectParticipantDto
            {
                PlacementId = p.Id,
                StudentUserId = p.StudentUserId,
                StudentName = p.StudentUser.Name,
                StudentEmail = p.StudentUser.Email,
                StudentRank = p.StudentRank,
                FacultyRank = p.FacultyRank,
                Source = p.Source,
            })
            .ToList();
    }
}
