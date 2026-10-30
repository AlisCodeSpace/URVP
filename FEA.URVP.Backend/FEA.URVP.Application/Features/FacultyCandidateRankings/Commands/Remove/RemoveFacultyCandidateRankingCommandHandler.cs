using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Commands.Semesters;
using FEA.URVP.Application.Projects;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.FacultyCandidateRankings.Remove;

public sealed class RemoveFacultyCandidateRankingCommandHandler
    : BaseCommandHandler<RemoveFacultyCandidateRankingCommand>
{
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ISemesterRepository _semesters;
    private readonly ProjectResearchAssistantAccess _access;

    public RemoveFacultyCandidateRankingCommandHandler(
        ILogger<RemoveFacultyCandidateRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFacultyCandidateRankingRepository candidateRankings,
        IProjectRepository projects,
        IUserRepository users,
        ISemesterRepository semesters,
        ProjectResearchAssistantAccess access)
        : base(logger, unitOfWork)
    {
        _candidateRankings = candidateRankings;
        _projects = projects;
        _users = users;
        _semesters = semesters;
        _access = access;
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

        var semester = await _semesters.FindActiveAsync(cancellationToken);
        ApplicationWindowRules.EnsureOpenForRanking(semester, DateTime.UtcNow);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ArgumentException("Project was not found.");

        await _access.EnsureCanRankCandidatesAsync(project, user, request.IsAdmin, cancellationToken);
        FacultyProjectMutationAccess.EnsureCanRankCandidates(project, request.IsAdmin);

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
