using FEA.URVP.Application.Features.AdminOverview.Queries;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IAdminOverviewReadRepository
{
    Task<AdminOverviewSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
