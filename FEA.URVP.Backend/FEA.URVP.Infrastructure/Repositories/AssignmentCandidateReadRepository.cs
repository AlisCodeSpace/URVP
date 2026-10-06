using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class AssignmentCandidateReadRepository : IAssignmentCandidateReadRepository
{
    private readonly AppDbContext _db;

    public AssignmentCandidateReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AssignmentStudentSource>> ListEligibleAsync(
        Guid projectId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var rows = await ApplySearch(Eligible(projectId), search)
            .OrderBy(row => row.Name)
            .ThenBy(row => row.UserId)
            .ToListAsync(cancellationToken);

        return rows.Select(ToSource).ToList();
    }

    public async Task<(IReadOnlyList<AssignmentStudentSource> Items, int TotalCount)> PageEligibleAsync(
        Guid projectId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySearch(Eligible(projectId), search);
        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(row => row.Name)
            .ThenBy(row => row.UserId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (rows.Select(ToSource).ToList(), totalCount);
    }

    public async Task<IReadOnlyDictionary<Guid, ConfirmedPlacementLabel>> LabelsForStudentsAsync(
        IReadOnlyCollection<Guid> studentUserIds,
        CancellationToken cancellationToken = default)
    {
        if (studentUserIds.Count == 0)
        {
            return new Dictionary<Guid, ConfirmedPlacementLabel>();
        }

        var rows = await _db.Placements
            .AsNoTracking()
            .Where(placement =>
                studentUserIds.Contains(placement.StudentUserId)
                && placement.Status == PlacementStatus.Confirmed
                && placement.MatchingRun.SemesterId == placement.Project.SemesterId
                && placement.Project.Status != ProjectStatus.Inactive)
            .Select(placement => new
            {
                placement.StudentUserId,
                placement.ProjectId,
                placement.Project.Title,
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.StudentUserId)
            .ToDictionary(
                group => group.Key,
                group => new ConfirmedPlacementLabel(group.First().ProjectId, group.First().Title));
    }

    /// <summary>
    /// Students only, minus anyone already confirmed on any current-cycle project.
    /// Profile columns are projected so the rest of the profile stays in the database.
    /// </summary>
    private IQueryable<CandidateRow> Eligible(Guid projectId)
    {
        _ = projectId;
        return
            from user in _db.Users.AsNoTracking()
            where user.Role == UserRole.Student
            where !_db.Placements.Any(placement =>
                placement.StudentUserId == user.Id
                && placement.Status == PlacementStatus.Confirmed
                && placement.MatchingRun.SemesterId == placement.Project.SemesterId
                && placement.Project.Status != ProjectStatus.Inactive)
            join profile in _db.StudentProfiles.AsNoTracking()
                on user.Id equals profile.UserId into profiles
            from profile in profiles.DefaultIfEmpty()
            select new CandidateRow
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                UserName = user.UserName,
                HasProfile = profile != null,
                Faculty = profile != null ? profile.Faculty : null,
                Major = profile != null ? profile.Major : null,
                Degree = profile != null ? profile.Degree : null,
                ResearchTopics = profile != null ? profile.ResearchTopics : null,
            };
    }

    private static IQueryable<CandidateRow> ApplySearch(IQueryable<CandidateRow> query, string? search)
    {
        var term = search?.Trim();
        if (string.IsNullOrEmpty(term))
        {
            return query;
        }

        var lowered = term.ToLower();
        return query.Where(row =>
            row.Name.ToLower().Contains(lowered)
            || row.Email.ToLower().Contains(lowered)
            || row.UserName.ToLower().Contains(lowered)
            || (row.Faculty != null && row.Faculty.ToLower().Contains(lowered))
            || (row.Major != null && row.Major.ToLower().Contains(lowered)));
    }

    private static AssignmentStudentSource ToSource(CandidateRow row) =>
        new(
            row.UserId,
            row.Name,
            row.Email,
            row.UserName,
            row.HasProfile,
            row.Faculty,
            row.Major,
            row.Degree,
            row.ResearchTopics ?? []);

    private sealed class CandidateRow
    {
        public Guid UserId { get; init; }

        public string Name { get; init; } = null!;

        public string Email { get; init; } = null!;

        public string UserName { get; init; } = null!;

        public bool HasProfile { get; init; }

        public string? Faculty { get; init; }

        public string? Major { get; init; }

        public string? Degree { get; init; }

        public List<string>? ResearchTopics { get; init; }
    }
}
