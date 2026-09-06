using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.ProjectRankings;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Rankings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ProjectRankings.Upsert;

public sealed class UpsertProjectRankingCommandHandler
    : BaseCommandHandler<UpsertProjectRankingCommand, ProjectRankingDto>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly IEventBus _eventBus;

    public UpsertProjectRankingCommandHandler(
        ILogger<UpsertProjectRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRankingRepository rankings,
        IProjectRepository projects,
        IUserRepository users,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _rankings = rankings;
        _projects = projects;
        _users = users;
        _eventBus = eventBus;
    }

    protected override async Task<ProjectRankingDto> HandleInternal(
        UpsertProjectRankingCommand request,
        CancellationToken cancellationToken)
    {
        var outcome = await UnitOfWork.ExecuteInTransactionAsync(
            ct => PersistAsync(request, ct),
            cancellationToken);

        await NotificationEventPublish.TryPublishAsync(
            _eventBus,
            outcome.Event,
            Logger,
            cancellationToken);

        return outcome.Dto;
    }

    private async Task<RankingOutcome> PersistAsync(
        UpsertProjectRankingCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        ProjectRankingAccess.EnsureCanRank(user.Role, user.Email);

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new ArgumentException("Project was not found.");

        if (project.Status != ProjectStatus.Open)
        {
            throw new InvalidOperationException("Only open projects can be ranked.");
        }

        if (project.VolunteersFilled >= project.VolunteersRequired)
        {
            throw new InvalidOperationException("This project has no open volunteer seats.");
        }

        var now = DateTime.UtcNow;
        var existingForProject = await _rankings.FindByStudentAndProjectAsync(
            user.Id,
            project.Id,
            cancellationToken);

        var occupant = await _rankings.FindByStudentAndRankAsync(
            user.Id,
            request.Rank,
            cancellationToken);

        if (occupant is not null && occupant.ProjectId != project.Id)
        {
            _rankings.Remove(occupant);
        }

        ProjectRanking ranking;
        if (existingForProject is null)
        {
            ranking = new ProjectRanking
            {
                StudentUserId = user.Id,
                ProjectId = project.Id,
                Rank = request.Rank,
                CreatedAt = now,
                UpdatedAt = now,
                Project = project,
            };
            _rankings.Add(ranking);

            Logger.LogInformation(
                "Student {UserId} ranked project {ProjectId} as #{Rank}",
                user.Id,
                project.Id,
                request.Rank);
        }
        else
        {
            ranking = existingForProject;
            ranking.Rank = request.Rank;
            ranking.UpdatedAt = now;
            ranking.Project = project;

            Logger.LogInformation(
                "Student {UserId} updated ranking for project {ProjectId} to #{Rank}",
                user.Id,
                project.Id,
                request.Rank);
        }

        var submitted = new ProjectRankingSubmittedEvent(
            ranking.Id,
            project.Id,
            project.CreatedByUserId,
            project.Title,
            user.Name);

        return new RankingOutcome(ranking.ToDto(), submitted);
    }

    private sealed record RankingOutcome(ProjectRankingDto Dto, ProjectRankingSubmittedEvent Event);
}
