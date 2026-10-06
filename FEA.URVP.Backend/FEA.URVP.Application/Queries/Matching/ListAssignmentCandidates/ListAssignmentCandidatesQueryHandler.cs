using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Matching;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;

public sealed class ListAssignmentCandidatesQueryHandler
    : IRequestHandler<ListAssignmentCandidatesQuery, (IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly IStudentProfileRepository _profiles;
    private readonly IMatchingRunRepository _runs;

    public ListAssignmentCandidatesQueryHandler(
        IProjectRepository projects,
        IUserRepository users,
        IStudentProfileRepository profiles,
        IMatchingRunRepository runs)
    {
        _projects = projects;
        _users = users;
        _profiles = profiles;
        _runs = runs;
    }

    public async Task<(IReadOnlyList<AssignmentCandidateDto> Items, int TotalCount)> Handle(
        ListAssignmentCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var students = await _users.ListAllAsync(
            null,
            UserRole.Student,
            UserSortField.Name,
            SortDirection.Asc,
            completedStudentProfilesOnly: false,
            facultyWithProjectsOnly: false,
            cancellationToken);

        var profiles = await _profiles.ListByUserIdsAsync(
            students.Select(student => student.Id).ToArray(),
            cancellationToken);
        var profileByUser = profiles.ToDictionary(profile => profile.UserId);

        var placements = await _runs.ListConfirmedByStudentIdsAsync(
            students.Select(student => student.Id).ToArray(),
            cancellationToken);
        var placementByUser = placements
            .GroupBy(placement => placement.StudentUserId)
            .ToDictionary(group => group.Key, group => group.First());

        var search = request.Search?.Trim();
        var ranked = students
            .Select(student => ToCandidate(
                student,
                project.ResearchAreas,
                project.MinQualifications,
                profileByUser.GetValueOrDefault(student.Id),
                placementByUser.GetValueOrDefault(student.Id)))
            .Where(candidate => candidate.Dto.AssignedProjectId != project.Id)
            .Where(candidate => MatchesSearch(candidate, search))
            .Where(candidate => !request.RecommendedOnly || candidate.Fit.IsRecommended);

        var ordered = request.RecommendedOnly
            ? ranked
                .OrderByDescending(candidate => candidate.Fit.Score)
                .ThenBy(candidate => candidate.Dto.Name, StringComparer.OrdinalIgnoreCase)
            : ranked.OrderBy(candidate => candidate.Dto.Name, StringComparer.OrdinalIgnoreCase);

        var matches = ordered.Select(candidate => candidate.Dto).ToList();
        var page = matches
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return (page, matches.Count);
    }

    private static bool MatchesSearch(ScoredCandidate candidate, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return Contains(candidate.Dto.Name, search)
            || Contains(candidate.Dto.Email, search)
            || Contains(candidate.UserName, search)
            || Contains(candidate.Dto.Faculty, search)
            || Contains(candidate.Dto.Major, search);
    }

    private static bool Contains(string? value, string search) =>
        value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;

    private static ScoredCandidate ToCandidate(
        User student,
        IReadOnlyList<string> researchAreas,
        string? minQualifications,
        StudentProfile? profile,
        Placement? placement)
    {
        var fit = AssignmentCandidateMatching.Evaluate(
            researchAreas,
            minQualifications,
            profile?.ResearchTopics,
            profile?.Major,
            profile?.Faculty);

        var dto = new AssignmentCandidateDto
        {
            UserId = student.Id,
            Name = student.Name,
            Email = student.Email,
            Faculty = profile?.Faculty,
            Major = profile?.Major,
            Degree = profile?.Degree,
            HasProfile = profile is not null,
            MatchedResearchTopics = fit.MatchedResearchTopics,
            QualificationsMentionMajor = fit.QualificationsMentionMajor,
            QualificationsMentionFaculty = fit.QualificationsMentionFaculty,
            AssignedProjectId = placement?.ProjectId,
            AssignedProjectTitle = placement?.Project.Title,
        };

        return new ScoredCandidate(dto, student.UserName, fit);
    }

    private sealed record ScoredCandidate(
        AssignmentCandidateDto Dto,
        string UserName,
        AssignmentCandidateMatching.Fit Fit);
}
