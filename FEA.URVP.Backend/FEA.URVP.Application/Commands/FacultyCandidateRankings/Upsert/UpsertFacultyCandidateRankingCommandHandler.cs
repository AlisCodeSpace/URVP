using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.FacultyCandidateRankings;
using FEA.URVP.Application.FacultyCandidateRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.FacultyCandidateRankings.Upsert;

public sealed class UpsertFacultyCandidateRankingCommandHandler
    : BaseCommandHandler<UpsertFacultyCandidateRankingCommand, FacultyCandidateRankingDto>
{
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly IProjectRankingRepository _projectRankings;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;

    public UpsertFacultyCandidateRankingCommandHandler(
        ILogger<UpsertFacultyCandidateRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFacultyCandidateRankingRepository candidateRankings,
        IProjectRankingRepository projectRankings,
        IProjectRepository projects,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _candidateRankings = candidateRankings;
        _projectRankings = projectRankings;
        _projects = projects;
        _users = users;
    }

    protected override bool UseTransaction => true;

    protected override async Task<FacultyCandidateRankingDto> HandleInternal(
        UpsertFacultyCandidateRankingCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        FacultyCandidateRankingAccess.EnsureCanRank(user.Role);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ArgumentException("Project was not found.");

        if (!request.IsAdmin && project.CreatedByUserId != user.Id)
        {
            throw new UnauthorizedAccessException("You can only rank candidates for your own projects.");
        }

        var application = await _projectRankings.FindByStudentAndProjectAsync(
            request.StudentUserId,
            project.Id,
            cancellationToken);

        if (application is null)
        {
            throw new InvalidOperationException(
                "You can only rank students who applied to this project.");
        }

        if (request.Rank < FacultyCandidateRanking.MinRank
            || request.Rank > FacultyCandidateRanking.MaxRank)
        {
            throw new InvalidOperationException(
                $"Rank must be between {FacultyCandidateRanking.MinRank} and {FacultyCandidateRanking.MaxRank}.");
        }

        var now = DateTime.UtcNow;
        var existing = await _candidateRankings.FindByProjectAndStudentAsync(
            project.Id,
            request.StudentUserId,
            cancellationToken);

        if (existing is null)
        {
            var ranking = new FacultyCandidateRanking
            {
                ProjectId = project.Id,
                StudentUserId = request.StudentUserId,
                Rank = request.Rank,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _candidateRankings.Add(ranking);

            Logger.LogInformation(
                "Faculty {UserId} ranked student {StudentUserId} as #{Rank} for project {ProjectId}",
                user.Id,
                request.StudentUserId,
                request.Rank,
                project.Id);

            return ranking.ToDto();
        }

        existing.Rank = request.Rank;
        existing.UpdatedAt = now;

        Logger.LogInformation(
            "Faculty {UserId} updated candidate ranking for student {StudentUserId} on project {ProjectId} to #{Rank}",
            user.Id,
            request.StudentUserId,
            project.Id,
            request.Rank);

        return existing.ToDto();
    }
}
