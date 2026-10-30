using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Exports;
using MediatR;

namespace FEA.URVP.Application.Queries.AdminOverview;

public sealed class ExportAdminOverviewQueryHandler
    : IRequestHandler<ExportAdminOverviewQuery, UserExportFileDto>
{
    private readonly IAdminOverviewReadRepository _overview;

    public ExportAdminOverviewQueryHandler(IAdminOverviewReadRepository overview)
    {
        _overview = overview;
    }

    public async Task<UserExportFileDto> Handle(
        ExportAdminOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await _overview.GetSnapshotAsync(cancellationToken);
        var overview = AdminOverviewAssembler.FromSnapshot(snapshot);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");

        return new UserExportFileDto
        {
            Content = OverviewExportDocuments.ToExcel(overview),
            MimeType = ExcelWorkbook.MimeType,
            FileName = $"urvp-overview-{stamp}.xlsx"
        };
    }
}
