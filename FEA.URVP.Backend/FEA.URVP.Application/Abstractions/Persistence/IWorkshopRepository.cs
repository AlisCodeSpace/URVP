using FEA.URVP.Domain.Entities.Workshops;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IWorkshopRepository
{
    Task<Workshop?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Workshop> Items, int TotalCount)> ListAsync(
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Workshop>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default);

    void Add(Workshop workshop);

    void Remove(Workshop workshop);
}
