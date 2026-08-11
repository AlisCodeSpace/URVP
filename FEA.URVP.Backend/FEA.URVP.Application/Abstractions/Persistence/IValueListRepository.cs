using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IValueListRepository
{
    Task<ValueListItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ValueListItem?> FindByKindAndNameAsync(
        ValueListKind kind,
        string name,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ValueListItem> Items, int TotalCount)> ListByKindAsync(
        ValueListKind kind,
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetActiveNamesAsync(
        ValueListKind kind,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSortOrderAsync(
        ValueListKind kind,
        CancellationToken cancellationToken = default);

    void Add(ValueListItem item);

    void Remove(ValueListItem item);
}
