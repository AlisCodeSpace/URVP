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

/// <summary>Admin user workbooks: basic account columns, or the full profile sheet.</summary>
public static class UserExportDocuments
{
    public static readonly string[] BasicHeaders = ["Name", "Email", "Role"];

    public static readonly string[] FullHeaders =
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

    private static readonly double[] BasicWidths = [28, 36, 14];

    private static readonly double[] FullWidths =
    [
        28, 36, 14, 36, 18, 28, 12, 18, 12, 42, 28, 18, 28, 24, 22, 16, 32, 36, 42, 16, 14,
    ];

    public static byte[] ToExcel(IReadOnlyList<UserExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            BasicHeaders,
            rows.Select(static row => (IReadOnlyList<string>)[row.Name, row.Email, row.Role]).ToList(),
            BasicWidths);

    public static byte[] ToFullExcel(IReadOnlyList<UserFullExportRow> rows) =>
        ExcelWorkbook.Build(
            "Users",
            FullHeaders,
            rows.Select(static row => row.Cells).ToList(),
            FullWidths);
}
