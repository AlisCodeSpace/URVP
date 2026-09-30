using System.IO.Compression;
using System.Text;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Exports;
using FEA.URVP.Application.Queries.AdminOverview;
using NSubstitute;

namespace FEA.URVP.Tests.AdminOverview;

public sealed class ExportAdminOverviewQueryHandlerTests
{
    [Fact]
    public async Task Export_writes_the_snapshot_counts_once()
    {
        var snapshot = new AdminOverviewSnapshot
        {
            UtcNow = new DateTime(2026, 9, 6, 16, 0, 0, DateTimeKind.Utc),
            Students = 12,
            StudentProfiles = 9,
            OpenProjects = 3,
            ConfirmedPlacements = 2,
        };

        var repo = Substitute.For<IAdminOverviewReadRepository>();
        repo.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(snapshot);

        var file = await new ExportAdminOverviewQueryHandler(repo).Handle(
            new ExportAdminOverviewQuery(),
            CancellationToken.None);

        Assert.Equal(ExcelWorkbook.MimeType, file.MimeType);
        Assert.EndsWith(".xlsx", file.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("urvp-overview-", file.FileName, StringComparison.Ordinal);

        var sheet = ReadSheet(file.Content);
        Assert.Contains("Student profiles", sheet, StringComparison.Ordinal);
        Assert.Contains(">12<", sheet, StringComparison.Ordinal);
        Assert.Equal(1, Count(sheet, "Student profiles"));
        Assert.DoesNotContain("Profiles saved", sheet, StringComparison.Ordinal);
        await repo.Received(1).GetSnapshotAsync(Arg.Any<CancellationToken>());
    }

    private static string ReadSheet(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);
        var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static int Count(string sheet, string value)
    {
        var needle = $">{value}<";
        var count = 0;
        var index = 0;
        while ((index = sheet.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }
}
