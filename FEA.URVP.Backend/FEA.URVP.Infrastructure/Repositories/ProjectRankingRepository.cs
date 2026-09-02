using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class ProjectRankingRepository : IProjectRankingRepository
{
    private readonly AppDbContext _db;

    public ProjectRankingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectRanking>> ListByStudentAsync(
        Guid studentUserId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectRankings
            .AsNoTracking()
            .Include(r => r.Project)
            .Where(r => r.StudentUserId == studentUserId)
            .OrderBy(r => r.Rank)
            .ToListAsync(cancellationToken);

    public Task<ProjectRanking?> FindByStudentAndProjectAsync(
        Guid studentUserId,
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        _db.ProjectRankings.FirstOrDefaultAsync(
            r => r.StudentUserId == studentUserId && r.ProjectId == projectId,
            cancellationToken);

    public Task<ProjectRanking?> FindByStudentAndRankAsync(
        Guid studentUserId,
        byte rank,
        CancellationToken cancellationToken = default) =>
        _db.ProjectRankings.FirstOrDefaultAsync(
            r => r.StudentUserId == studentUserId && r.Rank == rank,
            cancellationToken);

    public async Task<IReadOnlyList<ProjectRanking>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectRankings
            .AsNoTracking()
            .Include(r => r.StudentUser)
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.Rank)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> StudentHasRankedFacultyProjectAsync(
        Guid studentUserId,
        Guid facultyUserId,
        CancellationToken cancellationToken = default) =>
        _db.ProjectRankings
            .AsNoTracking()
            .AnyAsync(
                r => r.StudentUserId == studentUserId
                     && r.Project.CreatedByUserId == facultyUserId,
                cancellationToken);

    public async Task<IReadOnlyList<ProjectRanking>> ListByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
        {
            return [];
        }

        return await _db.ProjectRankings
            .AsNoTracking()
            .Include(r => r.StudentUser)
            .Where(r => projectIds.Contains(r.ProjectId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await _db.ProjectRankings
            .AsNoTracking()
            .Where(r => projectIds.Contains(r.ProjectId))
            .GroupBy(r => r.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.ProjectId, x => x.Count);
    }

    public void Add(ProjectRanking ranking) => _db.ProjectRankings.Add(ranking);

    public void Remove(ProjectRanking ranking) => _db.ProjectRankings.Remove(ranking);
}
