using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Matching;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.Run;

public sealed class RunMatchingCommandHandler
    : BaseCommandHandler<RunMatchingCommand, MatchingRunDetailDto>
{
    private const int MaxNamesInWarning = 5;

    private readonly ISemesterRepository _semesters;
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _studentRankings;
    private readonly IFacultyCandidateRankingRepository _facultyRankings;
    private readonly IMatchingRunRepository _runs;

    public RunMatchingCommandHandler(
        ILogger<RunMatchingCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISemesterRepository semesters,
        IProjectRepository projects,
        IProjectRankingRepository studentRankings,
        IFacultyCandidateRankingRepository facultyRankings,
        IMatchingRunRepository runs)
        : base(logger, unitOfWork)
    {
        _semesters = semesters;
        _projects = projects;
        _studentRankings = studentRankings;
        _facultyRankings = facultyRankings;
        _runs = runs;
    }

    protected override bool UseTransaction => true;

    protected override async Task<MatchingRunDetailDto> HandleInternal(
        RunMatchingCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var semester = request.SemesterId is Guid semesterId
            ? await _semesters.FindByIdAsync(semesterId, cancellationToken)
              ?? throw new KeyNotFoundException($"Semester {semesterId} was not found.")
            : await _semesters.FindActiveAsync(cancellationToken)
              ?? throw new InvalidOperationException("No active semester. Start a cycle before running matching.");

        var projects = await _projects.ListByStatusAsync(ProjectStatus.Open, cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();

        var confirmed = await _runs.ListConfirmedPlacementsAsync(semester.Id, cancellationToken);
        var placedStudents = confirmed.Select(p => p.StudentUserId).ToHashSet();
        var seatsTaken = confirmed
            .GroupBy(p => p.ProjectId)
            .ToDictionary(g => g.Key, g => g.Count());

        var capacity = projects.ToDictionary(
            p => p.Id,
            p => Math.Max(0, p.VolunteersRequired - seatsTaken.GetValueOrDefault(p.Id)));

        var studentRankings = (await _studentRankings.ListByProjectIdsAsync(projectIds, cancellationToken))
            .Where(r => !placedStudents.Contains(r.StudentUserId))
            .ToList();
        var facultyRankings = await _facultyRankings.ListByProjectIdsAsync(projectIds, cancellationToken);

        var warnings = BuildWarnings(semester.IsApplicationWindowOpen(now), projects, capacity, studentRankings, facultyRankings);

        var seed = request.Seed ?? Random.Shared.Next();
        var outcome = DeferredAcceptanceMatcher.Run(
            capacity,
            studentRankings.Select(r => new StudentPreference(r.StudentUserId, r.ProjectId, r.Rank)),
            facultyRankings.Select(r => new FacultyPreference(r.ProjectId, r.StudentUserId, r.Rank)),
            seed);

        foreach (var draft in await _runs.ListDraftsBySemesterAsync(semester.Id, cancellationToken))
        {
            draft.Discard(now);
        }

        var run = new MatchingRun
        {
            SemesterId = semester.Id,
            AlgorithmVersion = DeferredAcceptanceMatcher.Version,
            Seed = seed,
            StudentsConsidered = studentRankings.Select(r => r.StudentUserId).Distinct().Count(),
            ProjectsConsidered = capacity.Count(kv => kv.Value > 0),
            SeatsAvailable = capacity.Values.Sum(),
            StudentsMatched = outcome.Assignments.Count,
            TieBreaksUsed = outcome.TieBreaksUsed,
            Warnings = warnings,
            CreatedByUserId = request.CurrentUserId,
            CreatedAt = now,
            Placements = outcome.Assignments.Select(a => ToPlacement(a, now)).ToList(),
        };

        _runs.Add(run);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Matching run {RunId} for semester {SemesterId}: {Matched}/{Students} students placed across {Projects} projects (seed {Seed})",
            run.Id, semester.Id, run.StudentsMatched, run.StudentsConsidered, run.ProjectsConsidered, run.Seed);

        var saved = await _runs.GetDetailAsync(run.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Matching run {run.Id} was not found after save.");

        return saved.ToDetailDto();
    }

    private static Placement ToPlacement(MatchAssignment a, DateTime now) => new()
    {
        ProjectId = a.ProjectId,
        StudentUserId = a.StudentId,
        StudentRank = a.StudentRank,
        FacultyRank = a.FacultyRank,
        ResolvedByTieBreak = a.ResolvedByTieBreak,
        CreatedAt = now,
        UpdatedAt = now,
    };

    private static List<string> BuildWarnings(
        bool applicationWindowOpen,
        IReadOnlyList<Project> projects,
        IReadOnlyDictionary<Guid, int> capacity,
        IReadOnlyList<ProjectRanking> studentRankings,
        IReadOnlyList<FacultyCandidateRanking> facultyRankings)
    {
        var warnings = new List<string>();

        if (applicationWindowOpen)
        {
            warnings.Add("The student application window is still open; rankings may change after this run.");
        }

        var rankedByFaculty = facultyRankings.Select(r => (r.ProjectId, r.StudentUserId)).ToHashSet();
        var projectsWithFacultyRanks = facultyRankings.Select(r => r.ProjectId).ToHashSet();

        var unrankedProjects = projects
            .Where(p => capacity[p.Id] > 0
                        && !projectsWithFacultyRanks.Contains(p.Id)
                        && studentRankings.Any(r => r.ProjectId == p.Id))
            .Select(p => p.Title)
            .ToList();

        if (unrankedProjects.Count > 0)
        {
            warnings.Add(
                $"{unrankedProjects.Count} project(s) have applicants but no faculty rankings and were skipped: {Summarize(unrankedProjects)}.");
        }

        var fullProjects = projects.Where(p => capacity[p.Id] == 0).Select(p => p.Title).ToList();
        if (fullProjects.Count > 0)
        {
            warnings.Add($"{fullProjects.Count} open project(s) have no remaining seats: {Summarize(fullProjects)}.");
        }

        var unreachableStudents = studentRankings
            .GroupBy(r => r.StudentUserId)
            .Count(g => g.All(r => !rankedByFaculty.Contains((r.ProjectId, r.StudentUserId))));

        if (unreachableStudents > 0)
        {
            warnings.Add(
                $"{unreachableStudents} student(s) were not ranked by any faculty on their chosen projects and cannot be placed.");
        }

        return warnings;
    }

    private static string Summarize(IReadOnlyList<string> names)
    {
        var shown = string.Join(", ", names.Take(MaxNamesInWarning));
        return names.Count > MaxNamesInWarning ? $"{shown}, +{names.Count - MaxNamesInWarning} more" : shown;
    }
}
