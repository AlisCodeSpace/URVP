using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.ProjectRankings;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ProjectRankings.Upsert;

public sealed class UpsertProjectRankingCommandHandler
    : BaseCommandHandler<UpsertProjectRankingCommand, ProjectRankingDto>
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;

    public UpsertProjectRankingCommandHandler(
        ILogger<UpsertProjectRankingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRankingRepository rankings,
        IProjectRepository projects,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _rankings = rankings;
        _projects = projects;
        _users = users;
    }

    protected override bool UseTransaction => true;

    protected override async Task<ProjectRankingDto> HandleInternal(
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

        // Free the target rank slot if another project occupies it.
        var occupant = await _rankings.FindByStudentAndRankAsync(
            user.Id,
            request.Rank,
            cancellationToken);

        if (occupant is not null && occupant.ProjectId != project.Id)
        {
            _rankings.Remove(occupant);
        }

        if (existingForProject is null)
        {
            var ranking = new ProjectRanking
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

            return ranking.ToDto();
        }

        existingForProject.Rank = request.Rank;
        existingForProject.UpdatedAt = now;
        existingForProject.Project = project;

        Logger.LogInformation(
            "Student {UserId} updated ranking for project {ProjectId} to #{Rank}",
            user.Id,
            project.Id,
            request.Rank);

        return existingForProject.ToDto();
    }
}
