using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.ProjectRankings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Queries.Matching.GetMine;

public sealed class GetMyPlacementsQueryHandler
    : IRequestHandler<GetMyPlacementsQuery, IReadOnlyList<MyPlacementDto>>
{
    private readonly IMatchingRunRepository _runs;
    private readonly IUserRepository _users;
    private readonly ILogger<GetMyPlacementsQueryHandler> _logger;

    public GetMyPlacementsQueryHandler(
        IMatchingRunRepository runs,
        IUserRepository users,
        ILogger<GetMyPlacementsQueryHandler> logger)
    {
        _runs = runs;
        _users = users;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MyPlacementDto>> Handle(
        GetMyPlacementsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        ProjectRankingAccess.EnsureCanRank(user.Role, user.Email);

        var placements = await _runs.ListCurrentConfirmedByStudentAsync(user.Id, cancellationToken);
        _logger.LogDebug(
            "Loaded {Count} confirmed placements for student {UserId}",
            placements.Count,
            user.Id);

        return placements.Select(placement => placement.ToMyPlacementDto()).ToList();
    }
}
