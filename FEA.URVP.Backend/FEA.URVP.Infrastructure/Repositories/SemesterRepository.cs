using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class SemesterRepository : ISemesterRepository
{
    private readonly AppDbContext _db;

    public SemesterRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Semester?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Semesters.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Semester?> FindActiveAsync(CancellationToken cancellationToken = default) =>
        _db.Semesters.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

    public async Task<IReadOnlyList<Semester>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Semesters
            .AsNoTracking()
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task DeactivateAllExceptAsync(Guid exceptId, CancellationToken cancellationToken = default)
    {
        await _db.Semesters
            .Where(x => x.Id != exceptId && x.IsActive)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.IsActive, false)
                       .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public void Add(Semester semester) => _db.Semesters.Add(semester);

    public void Remove(Semester semester) => _db.Semesters.Remove(semester);
}
