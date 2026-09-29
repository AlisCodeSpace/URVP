using System.IO.Compression;
using System.Text;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Projects;
using FEA.URVP.Application.Queries.Projects.Export;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Projects;

public sealed class ExportAdminProjectsQueryHandlerTests
{
    [Fact]
    public async Task Export_includes_faculty_and_each_matched_student()
    {
        var projectId = Guid.NewGuid();
        var project = SampleProject(projectId, "Harbor Study");
        var projects = Substitute.For<IProjectRepository>();
        projects.ListAllForAdminAsync(
            Arg.Any<string?>(),
            Arg.Any<ProjectStatus?>(),
            Arg.Any<CancellationToken>()).Returns([project]);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedByProjectIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>()).Returns([
            Placement(projectId, "Lina Haddad", "lina@aub.edu.lb", PlacementStatus.Confirmed),
            Placement(projectId, "Omar Saleh", "omar@aub.edu.lb", PlacementStatus.Confirmed),
        ]);

        var file = await new ExportAdminProjectsQueryHandler(projects, runs, NoopClosure(projects)).Handle(
            new ExportAdminProjectsQuery("harbor", ProjectStatus.Open),
            CancellationToken.None);

        await projects.Received(1).ListAllForAdminAsync(
            "harbor",
            ProjectStatus.Open,
            Arg.Any<CancellationToken>());
        Assert.EndsWith(".xlsx", file.FileName, StringComparison.OrdinalIgnoreCase);

        var sheet = ReadSheet(file.Content);
        Assert.Contains("Harbor Study", sheet, StringComparison.Ordinal);
        Assert.Contains("Dr. Nour", sheet, StringComparison.Ordinal);
        Assert.Contains("Lina Haddad", sheet, StringComparison.Ordinal);
        Assert.Contains("Omar Saleh", sheet, StringComparison.Ordinal);
        Assert.Contains("Waves &amp; tides", sheet, StringComparison.Ordinal);
        Assert.Equal(3, CountRows(sheet));
    }

    [Fact]
    public async Task Declined_or_unmatched_students_are_not_listed_as_participants()
    {
        var projectId = Guid.NewGuid();
        var projects = Substitute.For<IProjectRepository>();
        projects.ListAllForAdminAsync(
            Arg.Any<string?>(),
            Arg.Any<ProjectStatus?>(),
            Arg.Any<CancellationToken>()).Returns([SampleProject(projectId, "Solo Lab")]);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedByProjectIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>()).Returns([
            Placement(projectId, "Ranked Only", "ranked@aub.edu.lb", PlacementStatus.Declined),
        ]);

        var file = await new ExportAdminProjectsQueryHandler(projects, runs, NoopClosure(projects)).Handle(
            new ExportAdminProjectsQuery(null, null),
            CancellationToken.None);

        var sheet = ReadSheet(file.Content);
        Assert.Contains("Solo Lab", sheet, StringComparison.Ordinal);
        Assert.Contains("Dr. Nour", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Ranked Only", sheet, StringComparison.Ordinal);
        Assert.Equal(2, CountRows(sheet));
    }

    private static ProjectCycleClosure NoopClosure(IProjectRepository projects)
    {
        projects.ListTrackedOnEndedCyclesAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);
        return new ProjectCycleClosure(
            projects,
            Substitute.For<IProjectRankingRepository>(),
            Substitute.For<IFacultyCandidateRankingRepository>(),
            Substitute.For<IMatchingRunRepository>(),
            Substitute.For<IProjectAlumniRepository>(),
            new FEA.URVP.Tests.Notifications.ImmediateUnitOfWork());
    }

    private static Project SampleProject(Guid id, string title) => new()
    {
        Id = id,
        Title = title,
        BriefDescription = "Waves & tides",
        FacultyNameSnapshot = "Dr. Nour",
        AffiliationSnapshot = "MSFEA",
        EmailSnapshot = "nour@aub.edu.lb",
        UserNameSnapshot = "nour",
        ResearchAreas = ["Water"],
        ActivityTypes = ["Fieldwork"],
        VolunteersRequired = 2,
        VolunteersFilled = 1,
        Status = ProjectStatus.Open,
        IrbStage = IrbStage.IrbApproved,
        CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    private static Placement Placement(
        Guid projectId,
        string name,
        string email,
        PlacementStatus status) => new()
    {
        ProjectId = projectId,
        Status = status,
        StudentRank = 1,
        FacultyRank = 1,
        StudentUser = new User
        {
            Name = name,
            Email = email,
            UserName = email.Split('@')[0],
            Affiliation = "AUB",
            Role = UserRole.Student,
        },
    };

    private static int CountRows(string sheet) =>
        sheet.Split("<row ", StringSplitOptions.None).Length - 1;

    private static string ReadSheet(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);
        var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry!.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
