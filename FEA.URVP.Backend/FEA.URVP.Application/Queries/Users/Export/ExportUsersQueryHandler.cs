using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Exports;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.Export;

public sealed class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, UserExportFileDto>
{
    public const string PdfMime = "application/pdf";
    public const string ExcelMime =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IUserRepository _users;

    public ExportUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserExportFileDto> Handle(
        ExportUsersQuery request,
        CancellationToken cancellationToken)
    {
        var format = NormalizeFormat(request.Format);
        var users = await _users.ListAllAsync(
            request.Search,
            request.Role,
            request.SortBy,
            request.SortDir,
            completedStudentProfilesOnly: true,
            cancellationToken);
        var rows = users
            .Select(user => new UserExportRow(
                user.Name,
                user.UserName,
                user.Email,
                UserMappings.ToLabel(user.Role)))
            .ToList();

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");

        return format switch
        {
            "pdf" => new UserExportFileDto
            {
                Content = UserExportDocuments.ToPdf(rows),
                MimeType = PdfMime,
                FileName = $"urvp-users-{stamp}.pdf"
            },
            "xlsx" => new UserExportFileDto
            {
                Content = UserExportDocuments.ToExcel(rows),
                MimeType = ExcelMime,
                FileName = $"urvp-users-{stamp}.xlsx"
            },
            _ => throw new ArgumentException("Export format must be pdf or excel.")
        };
    }

    private static string NormalizeFormat(string? format)
    {
        var value = format?.Trim().ToLowerInvariant() ?? "";
        return value is "excel" or "xlsx" ? "xlsx" : value;
    }
}
