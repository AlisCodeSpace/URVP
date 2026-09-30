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
    public bool FacultyWithProjectsOnly { get; }

    /// <summary>
    /// basic (name, email, role) or full. Full requires one role and includes
    /// only the fields that role can have.
    /// </summary>
    public string? Detail { get; }

    public ExportUsersQuery(
        string? format,
        string? search = null,
        UserRole? role = null,
        UserSortField sortBy = UserSortField.Name,
        SortDirection sortDir = SortDirection.Asc,
        bool facultyWithProjectsOnly = false,
        string? detail = null)
    {
        Format = format;
        Search = search;
        Role = role;
        SortBy = sortBy;
        SortDir = sortDir;
        FacultyWithProjectsOnly = facultyWithProjectsOnly;
        Detail = detail;
    }
}
