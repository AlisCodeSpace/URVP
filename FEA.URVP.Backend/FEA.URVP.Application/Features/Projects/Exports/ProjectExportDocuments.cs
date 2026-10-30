namespace FEA.URVP.Application.Exports;

public sealed record ProjectExportRow(
    string Title,
    string Status,
    string AcademicCycle,
    string ResearchAreas,
    string IrbStage,
    string BriefDescription,
    string ActivityTypes,
    string VolunteersRequired,
    string VolunteersFilled,
    string MinQualifications,
    string AdditionalComments,
    string Posted,
    string FacultyName,
    string FacultyEmail,
    string FacultyAffiliation,
    string FacultyUserName,
    string MatchedStudent,
    string MatchedStudentEmail,
    string MatchedStudentUserName)
{
    public IReadOnlyList<string> Cells =>
    [
        Title,
        Status,
        AcademicCycle,
        ResearchAreas,
        IrbStage,
        BriefDescription,
        ActivityTypes,
        VolunteersRequired,
        VolunteersFilled,
        MinQualifications,
        AdditionalComments,
        Posted,
        FacultyName,
        FacultyEmail,
        FacultyAffiliation,
        FacultyUserName,
        MatchedStudent,
        MatchedStudentEmail,
        MatchedStudentUserName,
    ];
}

/// <summary>
/// One workbook row per matched student. A project with no confirmed placement
/// is still included, with the student columns left blank.
/// </summary>
public static class ProjectExportDocuments
{
    public static readonly string[] Headers =
    [
        "Project title",
        "Status",
        "Academic cycle",
        "Research area",
        "IRB approval stage",
        "Brief description",
        "Research activity type",
        "Volunteers required",
        "Volunteers filled",
        "Minimum qualifications",
        "Additional comments",
        "Posted",
        "Faculty name",
        "Faculty email",
        "Faculty affiliation",
        "Faculty username",
        "Matched student",
        "Matched student email",
        "Matched student username",
    ];

    private static readonly double[] Widths =
    [
        36, 14, 24, 28, 32, 48, 28, 16, 16, 36, 36, 14, 24, 32, 28, 18, 24, 32, 22,
    ];

    public static byte[] ToExcel(IReadOnlyList<ProjectExportRow> rows) =>
        ExcelWorkbook.Build(
            "Projects",
            Headers,
            rows.Select(static row => row.Cells).ToList(),
            Widths);
}
