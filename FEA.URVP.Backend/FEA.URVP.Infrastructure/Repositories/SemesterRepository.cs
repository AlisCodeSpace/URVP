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

    public Task<Semester?> FindActiveAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return _db.Semesters
            .Where(x =>
                (x.CycleStart != null
                    && x.CycleStart <= now
                    && (x.CycleEnd == null || x.CycleEnd > now))
                || (x.CycleStart == null && x.IsActive))
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CycleStart)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Semester>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _db.Semesters
            .AsNoTracking()
            .OrderByDescending(x =>
                (x.CycleStart != null
                    && x.CycleStart <= now
                    && (x.CycleEnd == null || x.CycleEnd > now))
                || (x.CycleStart == null && x.IsActive))
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task RelinquishAllExceptAsync(
        Guid exceptId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var others = await _db.Semesters
            .Where(x => x.Id != exceptId)
            .ToListAsync(cancellationToken);

        foreach (var other in others)
        {
            var runningOrUpcoming =
                other.IsActive
                || other.IsCycleActive(utcNow)
                || (other.CycleStart.HasValue
                    && other.CycleStart.Value > utcNow
                    && (!other.CycleEnd.HasValue || other.CycleEnd.Value > utcNow));

            if (runningOrUpcoming)
                other.RelinquishCycle(utcNow);
        }
    }

    public Task<Semester?> FindOverlappingCycleAsync(
        Guid? excludeId,
        DateTime start,
        DateTime? end,
        CancellationToken cancellationToken = default)
    {
        var until = end ?? new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        return _db.Semesters
            .AsNoTracking()
            .Where(x => !excludeId.HasValue || x.Id != excludeId.Value)
            .Where(x => x.CycleStart != null)
            .Where(x =>
                x.CycleStart < until
                && (x.CycleEnd == null || start < x.CycleEnd))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(Semester semester) => _db.Semesters.Add(semester);

    public void Remove(Semester semester) => _db.Semesters.Remove(semester);
}
