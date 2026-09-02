using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.ProjectRankings.ListByProject;

public sealed class ListProjectRankingsQueryHandler
    : IRequestHandler<ListProjectRankingsQuery, IReadOnlyList<ProjectRankingStudentDto>>
{
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _rankings;
    private readonly IFacultyCandidateRankingRepository _candidateRankings;

    public ListProjectRankingsQueryHandler(
        IProjectRepository projects,
        IProjectRankingRepository rankings,
        IFacultyCandidateRankingRepository candidateRankings)
    {
        _projects = projects;
        _rankings = rankings;
        _candidateRankings = candidateRankings;
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

        if (!request.IsAdmin && project.CreatedByUserId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You can only view rankings for your own projects.");
        }

        var rankings = await _rankings.ListByProjectAsync(project.Id, cancellationToken);
        var facultyRanks = await _candidateRankings.ListByProjectAsync(project.Id, cancellationToken);
        return rankings.ToStudentDtos(facultyRanks);
    }
}
