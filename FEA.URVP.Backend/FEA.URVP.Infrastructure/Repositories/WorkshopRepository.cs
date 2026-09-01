using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Workshops;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class WorkshopRepository : IWorkshopRepository
{
    private readonly AppDbContext _db;

    public WorkshopRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Workshop?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Workshops.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Workshop> Items, int TotalCount)> ListAsync(
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Workshops.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term)
                || x.Description.Contains(term)
                || (x.Location != null && x.Location.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Workshop>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Workshops.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
    {
        var max = await _db.Workshops
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (max ?? -1) + 1;
    }

    public void Add(Workshop workshop) => _db.Workshops.Add(workshop);

    public void Remove(Workshop workshop) => _db.Workshops.Remove(workshop);
}
