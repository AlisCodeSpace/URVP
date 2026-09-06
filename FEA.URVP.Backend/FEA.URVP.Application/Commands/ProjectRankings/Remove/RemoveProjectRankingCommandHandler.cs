using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.ProjectRankings;
using FEA.URVP.Domain.Events.Rankings;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ProjectRankings.Remove;

public sealed class RemoveProjectRankingCommandHandler
    : BaseCommandHandler<RemoveProjectRankingCommand>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IFacultyCandidateRankingRepository _candidateRankings;
    private readonly IUserRepository _users;
    private readonly IProjectRepository _projects;
    private readonly IEventBus _eventBus;

    public RemoveProjectRankingCommandHandler(
        ILogger<RemoveProjectRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRankingRepository rankings,
        IFacultyCandidateRankingRepository candidateRankings,
        IUserRepository users,
        IProjectRepository projects,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _rankings = rankings;
        _candidateRankings = candidateRankings;
        _users = users;
        _projects = projects;
        _eventBus = eventBus;
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

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken);

        _rankings.Remove(ranking);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Student {UserId} removed ranking for project {ProjectId}",
            user.Id,
            request.ProjectId);

        if (project is not null)
        {
            await NotificationEventPublish.TryPublishAsync(
                _eventBus,
                new ProjectRankingRemovedEvent(
                    project.Id,
                    project.CreatedByUserId,
                    project.Title,
                    user.Name),
                Logger,
                cancellationToken);
        }
    }
}
