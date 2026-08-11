using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class ValueListRepository : IValueListRepository
{
    private readonly AppDbContext _db;

    public ValueListRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<ValueListItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ValueListItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ValueListItem?> FindByKindAndNameAsync(
        ValueListKind kind,
        string name,
        CancellationToken cancellationToken = default) =>
        _db.ValueListItems.FirstOrDefaultAsync(
            x => x.Kind == kind && x.Name == name,
            cancellationToken);

    public async Task<(IReadOnlyList<ValueListItem> Items, int TotalCount)> ListByKindAsync(
        ValueListKind kind,
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ValueListItems.AsNoTracking().Where(x => x.Kind == kind);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlySet<string>> GetActiveNamesAsync(
        ValueListKind kind,
        CancellationToken cancellationToken = default)
    {
        var names = await _db.ValueListItems.AsNoTracking()
            .Where(x => x.Kind == kind && x.IsActive)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        return names.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<int> GetNextSortOrderAsync(
        ValueListKind kind,
        CancellationToken cancellationToken = default)
    {
        var max = await _db.ValueListItems
            .Where(x => x.Kind == kind)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (max ?? -1) + 1;
    }

    public void Add(ValueListItem item) => _db.ValueListItems.Add(item);

    public void Remove(ValueListItem item) => _db.ValueListItems.Remove(item);
}
