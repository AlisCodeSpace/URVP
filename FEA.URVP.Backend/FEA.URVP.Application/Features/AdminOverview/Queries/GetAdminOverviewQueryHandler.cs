using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.AdminOverview.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.AdminOverview.Queries;

public sealed class GetAdminOverviewQueryHandler
    : IRequestHandler<GetAdminOverviewQuery, AdminOverviewDto>
{
    private readonly IAdminOverviewReadRepository _overview;

    public GetAdminOverviewQueryHandler(IAdminOverviewReadRepository overview)
    {
        _overview = overview;
    }

    public async Task<AdminOverviewDto> Handle(
        GetAdminOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _overview.GetSnapshotAsync(cancellationToken);
        return AdminOverviewAssembler.FromSnapshot(snapshot);
    }
}
