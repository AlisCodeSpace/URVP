using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.Export;

public sealed class ExportUsersQuery : IRequest<UserExportFileDto>
{
    public string? Format { get; }
    public string? Search { get; }
    public UserRole? Role { get; }
    public UserSortField SortBy { get; }
    public SortDirection SortDir { get; }

    public ExportUsersQuery(
        string? format,
        string? search = null,
        UserRole? role = null,
        UserSortField sortBy = UserSortField.Name,
        SortDirection sortDir = SortDirection.Asc)
    {
        Format = format;
        Search = search;
        Role = role;
        SortBy = sortBy;
        SortDir = sortDir;
    }
}
