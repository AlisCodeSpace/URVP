using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Divisions;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class DivisionRepository : IDivisionRepository
{
    private readonly AppDbContext _db;

    public DivisionRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Division?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Divisions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Division?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _db.Divisions.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

    public async Task<(IReadOnlyList<Division> Items, int TotalCount)> ListAsync(
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Divisions.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Name.Contains(term) || x.Description.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Division division) => _db.Divisions.Add(division);

    public void Remove(Division division) => _db.Divisions.Remove(division);
}
