using System.IO.Compression;
using System.Text;
using FEA.URVP.Application.Exports;

namespace FEA.URVP.Tests.Users;

public sealed class UserExportDocumentsTests
{
    private static readonly UserExportRow Sample = new("Ada Lovelace", "aa624", "aa624@mail.aub.edu", "Student");

    [Fact]
    public void Excel_contains_name_username_email_and_role()
    {
        var bytes = UserExportDocuments.ToExcel([
            Sample,
            new("Pat Instructor", "pi1", "pi1@aub.edu.lb", "Faculty")
        ]);
        var sheet = ReadSheet(bytes);

        Assert.Contains("Name", sheet, StringComparison.Ordinal);
        Assert.Contains("Username", sheet, StringComparison.Ordinal);
        Assert.Contains("Ada Lovelace", sheet, StringComparison.Ordinal);
        Assert.Contains("aa624", sheet, StringComparison.Ordinal);
        Assert.Contains("aa624@mail.aub.edu", sheet, StringComparison.Ordinal);
        Assert.Contains("Student", sheet, StringComparison.Ordinal);
        Assert.Contains("Pat Instructor", sheet, StringComparison.Ordinal);
        Assert.Contains("pi1@aub.edu.lb", sheet, StringComparison.Ordinal);
        Assert.Contains("Faculty", sheet, StringComparison.Ordinal);
    }

    [Fact]
    public void Excel_escapes_xml_special_characters()
    {
        var bytes = UserExportDocuments.ToExcel([new("A&B", "a&b", "x<y@aub.edu.lb", "Admin")]);
        var sheet = ReadSheet(bytes);

        Assert.Contains("A&amp;B", sheet, StringComparison.Ordinal);
        Assert.Contains("a&amp;b", sheet, StringComparison.Ordinal);
        Assert.Contains("x&lt;y@aub.edu.lb", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("a&b", sheet, StringComparison.Ordinal);
    }

    [Fact]
    public void Pdf_contains_name_username_email_and_role()
    {
        var bytes = UserExportDocuments.ToPdf([Sample]);
        var text = Encoding.ASCII.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", text, StringComparison.Ordinal);
        Assert.Contains("%%EOF", text, StringComparison.Ordinal);
        Assert.Contains("(Ada Lovelace)", text, StringComparison.Ordinal);
        Assert.Contains("(aa624)", text, StringComparison.Ordinal);
        Assert.Contains("(aa624@mail.aub.edu)", text, StringComparison.Ordinal);
        Assert.Contains("(Student)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Empty_exports_still_include_headers()
    {
        var excel = ReadSheet(UserExportDocuments.ToExcel([]));
        Assert.Contains("Name", excel, StringComparison.Ordinal);
        Assert.Contains("Username", excel, StringComparison.Ordinal);
        Assert.Contains("Email", excel, StringComparison.Ordinal);
        Assert.Contains("Role", excel, StringComparison.Ordinal);

        var pdf = Encoding.ASCII.GetString(UserExportDocuments.ToPdf([]));
        Assert.Contains("(Name)", pdf, StringComparison.Ordinal);
        Assert.Contains("(Username)", pdf, StringComparison.Ordinal);
        Assert.Contains("(Email)", pdf, StringComparison.Ordinal);
        Assert.Contains("(Role)", pdf, StringComparison.Ordinal);
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
