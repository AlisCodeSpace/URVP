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

    public ExportUsersQueryHandler(
        IUserRepository users,
        IStudentProfileRepository profiles,
        IMatchingRunRepository runs)
    {
        _users = users;
        _profiles = profiles;
        _runs = runs;
    }

    public async Task<UserExportFileDto> Handle(
        ExportUsersQuery request,
        CancellationToken cancellationToken)
    {
        NormalizeFormat(request.Format);
        var detail = NormalizeDetail(request.Detail);
        var users = await _users.ListAllAsync(
            request.Search,
            request.Role,
            request.SortBy,
            request.SortDir,
            completedStudentProfilesOnly: true,
            request.FacultyWithProjectsOnly,
            cancellationToken);

        var content = detail == "full"
            ? UserExportDocuments.ToFullExcel(await BuildFullRows(users, cancellationToken))
            : UserExportDocuments.ToExcel(users.Select(UserExportMapper.ToBasic).ToList());

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmm");
        return new UserExportFileDto
        {
            Content = content,
            MimeType = ExcelMime,
            FileName = $"urvp-users-{detail}-{stamp}.xlsx"
        };
    }

    private async Task<List<UserFullExportRow>> BuildFullRows(
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
