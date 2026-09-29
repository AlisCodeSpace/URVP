using System.IO.Compression;
using System.Text;
using FEA.URVP.Application.Exports;

namespace FEA.URVP.Tests.Users;

public sealed class UserExportDocumentsTests
{
    private static readonly UserExportRow Sample = new("Ada Lovelace", "aa624@mail.aub.edu", "Student");

    [Fact]
    public void Excel_contains_name_email_and_role()
    {
        var bytes = UserExportDocuments.ToExcel([
            Sample,
            new("Pat Instructor", "pi1@aub.edu.lb", "Faculty")
        ]);
        var sheet = ReadSheet(bytes);

        Assert.Contains("Name", sheet, StringComparison.Ordinal);
        Assert.Contains("Ada Lovelace", sheet, StringComparison.Ordinal);
        Assert.Contains("aa624@mail.aub.edu", sheet, StringComparison.Ordinal);
        Assert.Contains("Student", sheet, StringComparison.Ordinal);
        Assert.Contains("Pat Instructor", sheet, StringComparison.Ordinal);
        Assert.Contains("pi1@aub.edu.lb", sheet, StringComparison.Ordinal);
        Assert.Contains("Faculty", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Username", sheet, StringComparison.Ordinal);
    }

    [Fact]
    public void Excel_escapes_xml_special_characters()
    {
        var bytes = UserExportDocuments.ToExcel([new("A&B", "x<y@aub.edu.lb", "Admin")]);
        var sheet = ReadSheet(bytes);

        Assert.Contains("A&amp;B", sheet, StringComparison.Ordinal);
        Assert.Contains("x&lt;y@aub.edu.lb", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("A&B", sheet, StringComparison.Ordinal);
    }

    [Fact]
    public void Empty_exports_still_include_headers()
    {
        var excel = ReadSheet(UserExportDocuments.ToExcel([]));
        Assert.Contains("Name", excel, StringComparison.Ordinal);
        Assert.Contains("Email", excel, StringComparison.Ordinal);
        Assert.Contains("Role", excel, StringComparison.Ordinal);
    }

    [Fact]
    public void Full_excel_includes_matched_projects_and_profile_fields()
    {
        var bytes = UserExportDocuments.ToFullExcel([
            new UserFullExportRow(
                "Ada Lovelace",
                "ada@aub.edu.lb",
                "Student",
                "Harbor Study",
                "ada",
                "MSFEA",
                "Female",
                "70123456",
                "BEng",
                "Faculty of Engineering",
                "Civil Engineering",
                "2027",
                "English, Arabic",
                "",
                "Yes",
                "85.5",
                "Water",
                "A paper",
                "Monday: Morning (8:00–12:00)",
                "Yes",
                "No")
        ]);
        var sheet = ReadSheet(bytes);

        Assert.Contains("Matched projects", sheet, StringComparison.Ordinal);
        Assert.Contains("Harbor Study", sheet, StringComparison.Ordinal);
        Assert.Contains("Civil Engineering", sheet, StringComparison.Ordinal);
        Assert.Contains("Completed 24 credits at AUB", sheet, StringComparison.Ordinal);
        Assert.Contains("Transcript uploaded", sheet, StringComparison.Ordinal);
        Assert.Equal(UserExportDocuments.FullHeaders.Length, new UserFullExportRow(
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "").Cells.Count);
    }

    [Fact]
    public void Columns_past_z_use_two_letter_refs()
    {
        var headers = Enumerable.Range(1, 28).Select(i => $"H{i}").ToArray();
        var bytes = ExcelWorkbook.Build("Users", headers, []);
        var sheet = ReadSheet(bytes);

        Assert.Contains("r=\"AA1\"", sheet, StringComparison.Ordinal);
        Assert.Contains("r=\"AB1\"", sheet, StringComparison.Ordinal);
    }

    private static string ReadSheet(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);
        var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry!.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
