using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class MatchingRunRepository : IMatchingRunRepository
{
    private readonly AppDbContext _db;

    public MatchingRunRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<MatchingRun?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        QueryWithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<MatchingRun?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        QueryWithDetails().AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    private IQueryable<MatchingRun> QueryWithDetails() =>
        _db.MatchingRuns
            .Include(r => r.Semester)
            .Include(r => r.Placements).ThenInclude(p => p.Project)
            .Include(r => r.Placements).ThenInclude(p => p.StudentUser);

    public async Task<IReadOnlyList<MatchingRun>> ListAsync(
        Guid? semesterId,
        CancellationToken cancellationToken = default)
    {
        var query = _db.MatchingRuns.AsNoTracking().Include(r => r.Semester).AsQueryable();

        if (semesterId.HasValue)
        {
            query = query.Where(r => r.SemesterId == semesterId.Value);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MatchingRun>> ListDraftsBySemesterAsync(
        Guid semesterId,
        CancellationToken cancellationToken = default) =>
        await _db.MatchingRuns
            .Include(r => r.Placements)
            .Where(r => r.SemesterId == semesterId && r.Status == MatchingRunStatus.Draft)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Placement>> ListConfirmedPlacementsAsync(
        Guid semesterId,
        CancellationToken cancellationToken = default) =>
        await _db.Placements
            .AsNoTracking()
            .Where(p => p.MatchingRun.SemesterId == semesterId && p.Status == PlacementStatus.Confirmed)
            .ToListAsync(cancellationToken);

    public Task<Placement?> FindPlacementByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Placements
            .Include(p => p.Project)
            .Include(p => p.StudentUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<int> CountConfirmedByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        _db.Placements.CountAsync(
            p => p.ProjectId == projectId && p.Status == PlacementStatus.Confirmed,
            cancellationToken);

    public async Task<IReadOnlyList<Placement>> ListConfirmedByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.Placements
            .AsNoTracking()
            .Include(p => p.StudentUser)
            .Where(p => p.ProjectId == projectId && p.Status == PlacementStatus.Confirmed)
            .OrderBy(p => p.FacultyRank)
            .ThenBy(p => p.StudentUser.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListConfirmedProjectIdsByStudentAsync(
        Guid studentUserId,
        CancellationToken cancellationToken = default) =>
        await _db.Placements
            .AsNoTracking()
            .Where(p => p.StudentUserId == studentUserId && p.Status == PlacementStatus.Confirmed)
            .Select(p => p.ProjectId)
            .ToListAsync(cancellationToken);

    public void Add(MatchingRun run) => _db.MatchingRuns.Add(run);
}
