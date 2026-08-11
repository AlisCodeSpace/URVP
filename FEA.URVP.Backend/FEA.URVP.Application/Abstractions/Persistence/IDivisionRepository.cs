using FEA.URVP.Domain.Entities.Divisions;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IDivisionRepository
{
    Task<Division?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Division?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Division> Items, int TotalCount)> ListAsync(
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Division division);

    void Remove(Division division);
}
