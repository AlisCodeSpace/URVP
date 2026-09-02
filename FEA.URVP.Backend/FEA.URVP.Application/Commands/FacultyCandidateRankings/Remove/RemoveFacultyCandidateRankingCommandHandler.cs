using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.FacultyCandidateRankings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.FacultyCandidateRankings.Remove;

public sealed class RemoveFacultyCandidateRankingCommandHandler
    : BaseCommandHandler<RemoveFacultyCandidateRankingCommand>
{
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;

    public RemoveFacultyCandidateRankingCommandHandler(
        ILogger<RemoveFacultyCandidateRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFacultyCandidateRankingRepository candidateRankings,
        IProjectRepository projects,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _candidateRankings = candidateRankings;
        _projects = projects;
        _users = users;
    }

    protected override async Task HandleCommandAsync(
        RemoveFacultyCandidateRankingCommand request,
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

        var ranking = await _candidateRankings.FindByProjectAndStudentAsync(
            request.ProjectId,
            request.StudentUserId,
            cancellationToken)
            ?? throw new ArgumentException("Candidate ranking was not found for this student.");

        _candidateRankings.Remove(ranking);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Faculty {UserId} removed candidate ranking for student {StudentUserId} on project {ProjectId}",
            user.Id,
            request.StudentUserId,
            request.ProjectId);
    }
}
