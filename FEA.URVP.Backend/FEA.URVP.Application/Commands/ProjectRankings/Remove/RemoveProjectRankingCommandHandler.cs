using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.ProjectRankings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ProjectRankings.Remove;

public sealed class RemoveProjectRankingCommandHandler
    : BaseCommandHandler<RemoveProjectRankingCommand>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly IUserRepository _users;

    public RemoveProjectRankingCommandHandler(
        ILogger<RemoveProjectRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRankingRepository rankings,
        IFacultyCandidateRankingRepository candidateRankings,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _rankings = rankings;
        _candidateRankings = candidateRankings;
        _users = users;
    }

    protected override async Task HandleCommandAsync(
        RemoveProjectRankingCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        ProjectRankingAccess.EnsureCanRank(user.Role, user.Email);

        var ranking = await _rankings.FindByStudentAndProjectAsync(
            user.Id,
            request.ProjectId,
            cancellationToken)
            ?? throw new ArgumentException("Ranking was not found for this project.");

        var facultyRanking = await _candidateRankings.FindByProjectAndStudentAsync(
            request.ProjectId,
            user.Id,
            cancellationToken);
        if (facultyRanking is not null)
        {
            _candidateRankings.Remove(facultyRanking);
        }

        _rankings.Remove(ranking);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Student {UserId} removed ranking for project {ProjectId}",
            user.Id,
            request.ProjectId);
    }
}
