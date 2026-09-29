using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.ProjectRankings;
using FEA.URVP.Application.Projects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Queries.ProjectRankings.GetMine;

public sealed class GetMyProjectRankingsQueryHandler
    : IRequestHandler<GetMyProjectRankingsQuery, IReadOnlyList<ProjectRankingDto>>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IMatchingRunRepository _runs;
    private readonly IUserRepository _users;
    private readonly ISemesterRepository _semesters;
    private readonly ProjectCycleClosure _cycleClosure;
    private readonly ILogger<GetMyProjectRankingsQueryHandler> _logger;

    public GetMyProjectRankingsQueryHandler(
        IProjectRankingRepository rankings,
        IMatchingRunRepository runs,
        IUserRepository users,
        ISemesterRepository semesters,
        ProjectCycleClosure cycleClosure,
        ILogger<GetMyProjectRankingsQueryHandler> logger)
    {
        _rankings = rankings;
        _runs = runs;
        _users = users;
        _semesters = semesters;
        _cycleClosure = cycleClosure;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProjectRankingDto>> Handle(
        GetMyProjectRankingsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        ProjectRankingAccess.EnsureCanRank(user.Role, user.Email);

        await _cycleClosure.DeactivateEndedCyclesAsync(cancellationToken);

        var active = await _semesters.FindActiveAsync(cancellationToken);
        var rankings = (await _rankings.ListByStudentAsync(user.Id, cancellationToken))
            .Where(ranking => active is not null && ranking.Project.SemesterId == active.Id)
            .ToList();
        var matchedProjectIds = active is null
            ? []
            : (await _runs.ListConfirmedProjectIdsByStudentAsync(user.Id, active.Id, cancellationToken))
                .ToHashSet();
        _logger.LogDebug("Loaded {Count} rankings for student {UserId}", rankings.Count, user.Id);

        return rankings.Select(r => r.ToDto(matchedProjectIds.Contains(r.ProjectId))).ToList();
    }
}
