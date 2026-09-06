using FEA.URVP.Application.Queries.AdminOverview;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IAdminOverviewReadRepository
{
    Task<AdminOverviewSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
