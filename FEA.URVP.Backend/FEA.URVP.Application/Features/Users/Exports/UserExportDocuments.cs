namespace FEA.URVP.Application.Exports;

public sealed record UserExportRow(string Name, string Email, string Role);

public sealed record UserFullExportRow(
    string Name,
    string Email,
    string Role,
    string MatchedProjects,
    string UserName,
    string Affiliation,
    string Gender,
    string MobileNumber,
    string Degree,
    string Faculty,
    string Major,
    string ExpectedGraduationYear,
    string Languages,
    string OtherLanguages,
    string CompletedCredits,
    string CumulativeAverage,
    string ResearchTopics,
    string Publications,
    string Availability,
    string TranscriptUploaded,
    string CvUploaded)
{
    public IReadOnlyList<string> Cells =>
    [
        Name,
        Email,
        Role,
        MatchedProjects,
        UserName,
        Affiliation,
        Gender,
        MobileNumber,
        Degree,
        Faculty,
        Major,
        ExpectedGraduationYear,
        Languages,
        OtherLanguages,
        CompletedCredits,
        CumulativeAverage,
        ResearchTopics,
        Publications,
        Availability,
        TranscriptUploaded,
        CvUploaded,
    ];
}

public sealed record UserFacultyExportRow(
    string Name,
    string Email,
    string Role,
    string UserName,
    string Affiliation,
    string PostedProjects)
{
    public IReadOnlyList<string> Cells =>
    [
        Name,
        Email,
        Role,
        UserName,
        Affiliation,
        PostedProjects,
    ];
}

public sealed record UserAdminExportRow(
    string Name,
    string Email,
    string Role,
    string UserName,
    string Affiliation)
{
    public IReadOnlyList<string> Cells =>
    [
        Name,
        Email,
        Role,
        UserName,
        Affiliation,
    ];
}

/// <summary>
/// Admin user workbooks. Basic covers every role. Full sheets include only the
/// fields that exist for the selected role.
/// </summary>
public static class UserExportDocuments
{
    public static readonly string[] BasicHeaders = ["Name", "Email", "Role"];

    public static readonly string[] StudentHeaders =
    [
        "Name",
        "Email",
        "Role",
        "Matched projects",
        "Username",
        "Affiliation",
        "Gender",
        "Mobile number",
        "Degree",
        "Faculty",
        "Major",
        "Expected graduation year",
        "Languages",
        "Other languages",
        "Completed 24 credits at AUB",
        "Cumulative average",
        "Research topics",
        "Publications",
        "Availability",
        "Transcript uploaded",
        "CV uploaded",
    ];

    public static readonly string[] FacultyHeaders =
    [
        "Name",
        "Email",
        "Role",
        "Username",
        "Affiliation",
        "Posted projects",
    ];

    public static readonly string[] AdminHeaders =
    [
        "Name",
        "Email",
        "Role",
        "Username",
        "Affiliation",
    ];

    private static readonly double[] BasicWidths = [28, 36, 14];

    private static readonly double[] StudentWidths =
    [
        28, 36, 14, 36, 18, 28, 12, 18, 12, 42, 28, 18, 28, 24, 22, 16, 32, 36, 42, 16, 14,
    ];

    private static readonly double[] FacultyWidths = [28, 36, 14, 18, 28, 42];

    private static readonly double[] AdminWidths = [28, 36, 14, 18, 28];

    public static byte[] ToExcel(IReadOnlyList<UserExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            BasicHeaders,
            rows.Select(static row => (IReadOnlyList<string>)[row.Name, row.Email, row.Role]).ToList(),
            BasicWidths);

    public static byte[] ToStudentExcel(IReadOnlyList<UserFullExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            StudentHeaders,
            rows.Select(static row => row.Cells).ToList(),
            StudentWidths);

    public static byte[] ToFacultyExcel(IReadOnlyList<UserFacultyExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            FacultyHeaders,
            rows.Select(static row => row.Cells).ToList(),
            FacultyWidths);

    public static byte[] ToAdminExcel(IReadOnlyList<UserAdminExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            AdminHeaders,
            rows.Select(static row => row.Cells).ToList(),
            AdminWidths);
}
