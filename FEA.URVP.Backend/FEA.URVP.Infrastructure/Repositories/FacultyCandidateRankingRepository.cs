using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class FacultyCandidateRankingRepository : IFacultyCandidateRankingRepository
{
    private readonly AppDbContext _db;

    public FacultyCandidateRankingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FacultyCandidateRanking>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.FacultyCandidateRankings
            .AsNoTracking()
            .Include(r => r.StudentUser)
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.Rank)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FacultyCandidateRanking>> ListByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
        {
            return [];
        }

        return await _db.FacultyCandidateRankings
            .AsNoTracking()
            .Where(r => projectIds.Contains(r.ProjectId))
            .ToListAsync(cancellationToken);
    }

    public Task<FacultyCandidateRanking?> FindByProjectAndStudentAsync(
        Guid projectId,
        Guid studentUserId,
        CancellationToken cancellationToken = default) =>
        _db.FacultyCandidateRankings.FirstOrDefaultAsync(
            r => r.ProjectId == projectId && r.StudentUserId == studentUserId,
            cancellationToken);

    public void Add(FacultyCandidateRanking ranking) => _db.FacultyCandidateRankings.Add(ranking);

    public void Remove(FacultyCandidateRanking ranking) => _db.FacultyCandidateRankings.Remove(ranking);
}
