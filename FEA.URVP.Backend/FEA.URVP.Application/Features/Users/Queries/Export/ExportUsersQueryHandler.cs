using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Exports;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.Export;

public sealed class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, UserExportFileDto>
{
    public const string ExcelMime = ExcelWorkbook.MimeType;

    private readonly IUserRepository _users;
    private readonly IStudentProfileRepository _profiles;
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectRepository _projects;

    public ExportUsersQueryHandler(
        IUserRepository users,
        IStudentProfileRepository profiles,
        IMatchingRunRepository runs,
        IProjectRepository projects)
    {
        _users = users;
        _profiles = profiles;
        _runs = runs;
        _projects = projects;
    }

    public async Task<UserExportFileDto> Handle(
        ExportUsersQuery request,
        CancellationToken cancellationToken)
    {
        NormalizeFormat(request.Format);
        var detail = NormalizeDetail(request.Detail);
        if (detail == "full" && request.Role is null)
        {
            throw new ArgumentException("Full export requires a single role.");
        }

        var users = await _users.ListAllAsync(
            request.Search,
            request.Role,
            request.SortBy,
            request.SortDir,
            completedStudentProfilesOnly: true,
            request.FacultyWithProjectsOnly,
            cancellationToken);

        var content = await BuildWorkbook(detail, request.Role, users, cancellationToken);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");
        var scope = detail == "full" && request.Role is not null
            ? $"{detail}-{request.Role.Value.ToString().ToLowerInvariant()}"
            : detail;
        return new UserExportFileDto
        {
            Content = content,
            MimeType = ExcelMime,
            FileName = $"urvp-users-{scope}-{stamp}.xlsx"
        };
    }

    private async Task<byte[]> BuildWorkbook(
        string detail,
        UserRole? role,
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        if (detail != "full")
        {
            return UserExportDocuments.ToExcel(users.Select(UserExportMapper.ToBasic).ToList());
        }

        return role switch
        {
            UserRole.Student => UserExportDocuments.ToStudentExcel(
                await BuildStudentRows(users, cancellationToken)),
            UserRole.Faculty => UserExportDocuments.ToFacultyExcel(
                await BuildFacultyRows(users, cancellationToken)),
            UserRole.Admin => UserExportDocuments.ToAdminExcel(
                users.Select(UserExportMapper.ToAdmin).ToList()),
            _ => throw new ArgumentException("Full export requires a single role."),
        };
    }

    private async Task<List<UserFacultyExportRow>> BuildFacultyRows(
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        var facultyIds = users
            .Where(user => user.Role == UserRole.Faculty)
            .Select(user => user.Id)
            .ToArray();

        IReadOnlyList<PostedProjectTitle> titles = facultyIds.Length == 0
            ? []
            : await _projects.ListPostedTitlesByCreatorIdsAsync(facultyIds, cancellationToken);

        var titlesByFaculty = titles
            .Where(item => !string.IsNullOrWhiteSpace(item.Title))
            .GroupBy(item => item.CreatedByUserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(item => item.Title)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(title => title, StringComparer.OrdinalIgnoreCase)
                    .ToArray());

        return users
            .Select(user => UserExportMapper.ToFaculty(
                user,
                titlesByFaculty.GetValueOrDefault(user.Id) ?? []))
            .ToList();
    }

    private async Task<List<UserFullExportRow>> BuildStudentRows(
        IReadOnlyList<User> users,
        CancellationToken cancellationToken)
    {
        var studentIds = users
            .Where(user => user.Role == UserRole.Student)
            .Select(user => user.Id)
            .ToArray();

        IReadOnlyList<StudentProfile> profiles = studentIds.Length == 0
            ? []
            : await _profiles.ListByUserIdsAsync(studentIds, cancellationToken);
        IReadOnlyList<Placement> placements = studentIds.Length == 0
            ? []
            : await _runs.ListConfirmedByStudentIdsAsync(studentIds, cancellationToken);

        var profileByUser = profiles
            .GroupBy(profile => profile.UserId)
            .ToDictionary(group => group.Key, group => group.First());
        var projectsByStudent = placements
            .Where(placement => placement.Status == PlacementStatus.Confirmed && placement.Project is not null)
            .GroupBy(placement => placement.StudentUserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(placement => placement.Project.Title)
                    .Where(title => !string.IsNullOrWhiteSpace(title))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(title => title, StringComparer.OrdinalIgnoreCase)
                    .ToArray());

        return users
            .Select(user => UserExportMapper.ToFull(
                user,
                profileByUser.GetValueOrDefault(user.Id),
                projectsByStudent.GetValueOrDefault(user.Id) ?? []))
            .ToList();
    }

    private static void NormalizeFormat(string? format)
    {
        var value = format?.Trim().ToLowerInvariant() ?? "";
        if (value is "excel" or "xlsx")
        {
            return;
        }

        throw new ArgumentException("Export format must be excel.");
    }

    private static string NormalizeDetail(string? detail)
    {
        var value = detail?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(value) || value == "basic")
        {
            return "basic";
        }

        if (value == "full")
        {
            return "full";
        }

        throw new ArgumentException("Export detail must be basic or full.");
    }
}
