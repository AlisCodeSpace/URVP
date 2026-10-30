using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Common.Exports;
using FEA.URVP.Application.Features.AdminOverview.Exports;
using FEA.URVP.Application.Features.Users.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.AdminOverview.Queries;

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
