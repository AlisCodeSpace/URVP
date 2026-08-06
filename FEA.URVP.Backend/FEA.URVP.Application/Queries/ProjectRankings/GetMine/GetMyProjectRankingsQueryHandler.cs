using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.ProjectRankings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Queries.ProjectRankings.GetMine;

public sealed class GetMyProjectRankingsQueryHandler
    : IRequestHandler<GetMyProjectRankingsQuery, IReadOnlyList<ProjectRankingDto>>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IUserRepository _users;
    private readonly ILogger<GetMyProjectRankingsQueryHandler> _logger;

    public GetMyProjectRankingsQueryHandler(
        IProjectRankingRepository rankings,
        IUserRepository users,
        ILogger<GetMyProjectRankingsQueryHandler> logger)
    {
        _rankings = rankings;
        _users = users;
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

        var rankings = await _rankings.ListByStudentAsync(user.Id, cancellationToken);
        _logger.LogDebug("Loaded {Count} rankings for student {UserId}", rankings.Count, user.Id);

        return rankings.Select(r => r.ToDto()).ToList();
    }
}
