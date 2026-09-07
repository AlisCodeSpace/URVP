using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Matching;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Matching;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.Assign;

public sealed class AssignStudentToProjectCommandHandler
    : BaseCommandHandler<AssignStudentToProjectCommand, PlacementDto>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ISemesterRepository _semesters;
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRankingRepository _studentRankings;
    private readonly IFacultyCandidateRankingRepository _facultyRankings;
    private readonly IEventBus _eventBus;

    public AssignStudentToProjectCommandHandler(
        ILogger<AssignStudentToProjectCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IProjectRepository projects,
        IUserRepository users,
        ISemesterRepository semesters,
        IMatchingRunRepository runs,
        IProjectRankingRepository studentRankings,
        IFacultyCandidateRankingRepository facultyRankings,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _projects = projects;
        _users = users;
        _semesters = semesters;
        _runs = runs;
        _studentRankings = studentRankings;
        _facultyRankings = facultyRankings;
        _eventBus = eventBus;
    }

    protected override async Task<PlacementDto> HandleInternal(
        AssignStudentToProjectCommand request,
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

    private async Task<AssignOutcome> PersistAsync(
        AssignStudentToProjectCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        if (project.Status == ProjectStatus.Closed)
        {
            throw new InvalidOperationException("Closed projects cannot accept new assignments.");
        }

        var student = await _users.FindByIdAsync(request.StudentUserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.StudentUserId} was not found.");

        if (student.Role != UserRole.Student)
        {
            throw new InvalidOperationException("Only students can be assigned to a project.");
        }

        var confirmedProjectIds = await _runs.ListConfirmedProjectIdsByStudentAsync(
            student.Id,
            cancellationToken);

        if (confirmedProjectIds.Contains(project.Id))
        {
            throw new InvalidOperationException("This student is already assigned to this project.");
        }

        if (confirmedProjectIds.Count > 0)
        {
            var other = await _projects.FindByIdAsync(confirmedProjectIds[0], cancellationToken);
            var title = other?.Title ?? "another project";
            throw new InvalidOperationException($"This student is already assigned to \"{title}\".");
        }

        var filled = await _runs.CountConfirmedByProjectAsync(project.Id, cancellationToken);
        if (filled >= project.VolunteersRequired)
        {
            throw new InvalidOperationException(
                $"This project is full ({project.VolunteersRequired} seat{(project.VolunteersRequired == 1 ? "" : "s")}).");
        }

        var semester = await _semesters.FindActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "No active URVP cycle. Start a cycle before assigning students.");

        var run = await _runs.FindManualBySemesterAsync(semester.Id, cancellationToken);
        if (run is null)
        {
            run = CreateManualRun(semester.Id, request.CurrentUserId, now);
            _runs.Add(run);
        }

        var studentRank = (await _studentRankings.FindByStudentAndProjectAsync(
            student.Id, project.Id, cancellationToken))?.Rank ?? (byte)0;
        var facultyRank = (await _facultyRankings.FindByProjectAndStudentAsync(
            project.Id, student.Id, cancellationToken))?.Rank ?? (byte)0;

        var placement = run.Placements.FirstOrDefault(p => p.StudentUserId == student.Id);
        if (placement is null)
        {
            placement = new Placement
            {
                MatchingRunId = run.Id,
                ProjectId = project.Id,
                Project = project,
                StudentUserId = student.Id,
                StudentUser = student,
                StudentRank = studentRank,
                FacultyRank = facultyRank,
                ResolvedByTieBreak = false,
                Source = PlacementSource.Manual,
                Status = PlacementStatus.Confirmed,
                CreatedAt = now,
                UpdatedAt = now,
            };
            run.Placements.Add(placement);
        }
        else
        {
            placement.ProjectId = project.Id;
            placement.Project = project;
            placement.StudentUser = student;
            placement.StudentRank = studentRank;
            placement.FacultyRank = facultyRank;
            placement.ResolvedByTieBreak = false;
            placement.Source = PlacementSource.Manual;
            placement.SetStatus(PlacementStatus.Confirmed, now);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        await ProjectSeatSync.ApplyAsync([project.Id], _projects, _runs, cancellationToken);

        Logger.LogInformation(
            "Admin {AdminId} assigned student {StudentId} to project {ProjectId} (placement {PlacementId})",
            request.CurrentUserId, student.Id, project.Id, placement.Id);

        return new AssignOutcome(placement.ToDto(), new PlacementAssignedEvent(placement.Id));
    }

    private static MatchingRun CreateManualRun(Guid semesterId, Guid adminUserId, DateTime now) => new()
    {
        SemesterId = semesterId,
        Status = MatchingRunStatus.Confirmed,
        AlgorithmVersion = MatchingRun.ManualAlgorithmVersion,
        Seed = 0,
        Warnings = [],
        CreatedByUserId = adminUserId,
        ConfirmedByUserId = adminUserId,
        CreatedAt = now,
        ConfirmedAt = now,
    };

    private sealed record AssignOutcome(PlacementDto Dto, PlacementAssignedEvent Event);
}
