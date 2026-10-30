using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Projects;
using MediatR;

namespace FEA.URVP.Application.Queries.ProjectRankings.ListByProject;

public sealed class ListProjectRankingsQueryHandler
    : IRequestHandler<ListProjectRankingsQuery, IReadOnlyList<ProjectRankingStudentDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _rankings;
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly ProjectResearchAssistantAccess _access;

    public ListProjectRankingsQueryHandler(
        IProjectRepository projects,
        IProjectRankingRepository rankings,
        IFacultyCandidateRankingRepository candidateRankings,
        ProjectResearchAssistantAccess access)
    {
        _projects = projects;
        _rankings = rankings;
        _candidateRankings = candidateRankings;
        _access = access;
    }

    public async Task<IReadOnlyList<ProjectRankingStudentDto>> Handle(
        ListProjectRankingsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        await _access.EnsureCanReviewAsync(
            project,
            request.CurrentUserId,
            request.IsAdmin,
            "You can only view rankings for a project you posted or assist.",
            cancellationToken);

        var rankings = await _rankings.ListByProjectAsync(project.Id, cancellationToken);
        var facultyRanks = await _candidateRankings.ListByProjectAsync(project.Id, cancellationToken);
        return rankings.ToStudentDtos(facultyRanks);
    }
}
