using System.Globalization;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Application.Exports;

public static class UserExportMapper
{
    private static readonly string[] WeekdayOrder =
    [
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday",
    ];

    public static UserExportRow ToBasic(User user) =>
        new(user.Name, user.Email, UserMappings.ToLabel(user.Role));

    public static UserFullExportRow ToFull(
        User user,
        StudentProfile? profile,
        IReadOnlyList<string> matchedProjectTitles) =>
        new(
            user.Name,
            user.Email,
            UserMappings.ToLabel(user.Role),
            Join(matchedProjectTitles, "; "),
            user.UserName,
            user.Affiliation,
            profile?.Gender ?? "",
            profile?.MobileNumber ?? "",
            profile?.Degree ?? "",
            profile?.Faculty ?? "",
            profile?.Major ?? "",
            profile is null
                ? ""
                : profile.ExpectedGraduationYear.ToString(CultureInfo.InvariantCulture),
            Join(profile?.Languages, ", "),
            profile?.OtherLanguages ?? "",
            profile is null ? "" : YesNo(profile.CompletedCredits),
            profile is null
                ? ""
                : profile.CumulativeAverage.ToString("0.##", CultureInfo.InvariantCulture),
            Join(profile?.ResearchTopics, ", "),
            profile?.Publications ?? "",
            FormatAvailability(profile?.Availability),
            profile is null ? "" : YesNo(profile.TranscriptFileId.HasValue),
            profile is null ? "" : YesNo(profile.CvFileId.HasValue));

    public static UserFacultyExportRow ToFaculty(
        User user,
        IReadOnlyList<string> postedProjectTitles) =>
        new(
            user.Name,
            user.Email,
            UserMappings.ToLabel(user.Role),
            user.UserName,
            user.Affiliation,
            Join(postedProjectTitles, "; "));

    public static UserAdminExportRow ToAdmin(User user) =>
        new(
            user.Name,
            user.Email,
            UserMappings.ToLabel(user.Role),
            user.UserName,
            user.Affiliation);

    private static string YesNo(bool value) => value ? "Yes" : "No";

    private static string Join(IEnumerable<string>? values, string separator) =>
        values is null
            ? ""
            : string.Join(separator, values.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string FormatAvailability(IReadOnlyList<DayAvailability>? availability)
    {
        if (availability is null || availability.Count == 0)
        {
            return "";
        }

        return string.Join(
            "; ",
            availability
                .Where(entry => entry.Slots.Count > 0)
                .OrderBy(entry =>
                {
                    var index = Array.IndexOf(WeekdayOrder, entry.Day);
                    return index < 0 ? int.MaxValue : index;
                })
                .ThenBy(entry => entry.Day, StringComparer.Ordinal)
                .Select(entry =>
                    $"{entry.Day}: {string.Join(", ", entry.Slots.Where(slot => !string.IsNullOrWhiteSpace(slot)))}"));
    }
}
