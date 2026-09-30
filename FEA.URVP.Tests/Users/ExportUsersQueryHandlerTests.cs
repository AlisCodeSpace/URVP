using System.IO.Compression;
using System.Text;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Users.Export;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Users;

public sealed class ExportUsersQueryHandlerTests
{
    [Theory]
    [InlineData("xlsx")]
    [InlineData("excel")]
    [InlineData("Excel")]
    public async Task Excel_export_returns_a_workbook(string format)
    {
        var file = await Handle(format);

        Assert.Equal(ExportUsersQueryHandler.ExcelMime, file.MimeType);
        Assert.EndsWith(".xlsx", file.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("urvp-users-basic-", file.FileName, StringComparison.Ordinal);
        Assert.Equal((byte)'P', file.Content[0]);
        Assert.Equal((byte)'K', file.Content[1]);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData("csv")]
    public async Task Non_excel_formats_are_rejected(string format)
    {
        var handler = Handler(Users());

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ExportUsersQuery(format), CancellationToken.None));

        Assert.Contains("excel", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_detail_is_rejected()
    {
        var handler = Handler(Users());

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ExportUsersQuery("xlsx", detail: "summary"), CancellationToken.None));

        Assert.Contains("basic or full", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_and_role_filters_are_forwarded_to_the_repository()
    {
        var users = Users();
        var profiles = Substitute.For<IStudentProfileRepository>();
        var handler = Handler(users, profiles);

        await handler.Handle(
            new ExportUsersQuery(
                "xlsx",
                "ada",
                UserRole.Student,
                UserSortField.Email,
                SortDirection.Desc),
            CancellationToken.None);

        await users.Received(1).ListAllAsync(
            "ada",
            UserRole.Student,
            UserSortField.Email,
            SortDirection.Desc,
            true,
            false,
            Arg.Any<CancellationToken>());
        await profiles.DidNotReceive().ListByUserIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Faculty_with_projects_filter_is_forwarded_to_the_repository()
    {
        var users = Users();
        var handler = Handler(users);

        await handler.Handle(
            new ExportUsersQuery("xlsx", facultyWithProjectsOnly: true),
            CancellationToken.None);

        await users.Received(1).ListAllAsync(
            null,
            null,
            UserSortField.Name,
            SortDirection.Asc,
            true,
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Full_export_without_a_role_is_rejected()
    {
        var handler = Handler(Users());

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ExportUsersQuery("xlsx", detail: "full"), CancellationToken.None));

        Assert.Contains("single role", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Full_export_includes_the_profile_and_confirmed_project()
    {
        var userId = Guid.NewGuid();
        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
            Arg.Any<string?>(),
            Arg.Any<UserRole?>(),
            Arg.Any<UserSortField>(),
            Arg.Any<SortDirection>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()).Returns([
            new User
            {
                Id = userId,
                Email = "ada@aub.edu.lb",
                Name = "Ada Lovelace",
                UserName = "ada",
                Affiliation = "MSFEA",
                Role = UserRole.Student,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);

        var profiles = Substitute.For<IStudentProfileRepository>();
        profiles.ListByUserIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>()).Returns([
            new StudentProfile
            {
                UserId = userId,
                Gender = "Female",
                MobileNumber = "70123456",
                Degree = "BEng",
                Faculty = "MSFEA",
                Major = "Civil Engineering",
                ExpectedGraduationYear = 2027,
                Languages = ["English"],
                CompletedCredits = true,
                CumulativeAverage = 85.5m,
                ResearchTopics = ["Water"],
                Availability = []
            }
        ]);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedByStudentIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>()).Returns([
            new Placement
            {
                StudentUserId = userId,
                Status = PlacementStatus.Confirmed,
                Project = new Project { Title = "Harbor Study" }
            }
        ]);

        var file = await Handler(users, profiles, runs).Handle(
            new ExportUsersQuery("xlsx", role: UserRole.Student, detail: "full"),
            CancellationToken.None);

        Assert.Contains("urvp-users-full-student-", file.FileName, StringComparison.Ordinal);
        var sheet = ReadSheet(file.Content);
        Assert.Contains("Harbor Study", sheet, StringComparison.Ordinal);
        Assert.Contains("Civil Engineering", sheet, StringComparison.Ordinal);
        Assert.Contains("85.5", sheet, StringComparison.Ordinal);
        Assert.Contains("Matched projects", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Posted projects", sheet, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Faculty_full_export_includes_posted_projects_only()
    {
        var userId = Guid.NewGuid();
        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
            Arg.Any<string?>(),
            Arg.Any<UserRole?>(),
            Arg.Any<UserSortField>(),
            Arg.Any<SortDirection>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()).Returns([
            new User
            {
                Id = userId,
                Email = "pi1@aub.edu.lb",
                Name = "Pat Instructor",
                UserName = "pi1",
                Affiliation = "MSFEA",
                Role = UserRole.Faculty,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);

        var profiles = Substitute.For<IStudentProfileRepository>();
        var projects = Substitute.For<IProjectRepository>();
        projects.ListPostedTitlesByCreatorIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>()).Returns([
            new PostedProjectTitle(userId, "Harbor Study")
        ]);

        var file = await Handler(users, profiles, projects: projects).Handle(
            new ExportUsersQuery("xlsx", role: UserRole.Faculty, detail: "full"),
            CancellationToken.None);

        Assert.Contains("urvp-users-full-faculty-", file.FileName, StringComparison.Ordinal);
        var sheet = ReadSheet(file.Content);
        Assert.Contains("Posted projects", sheet, StringComparison.Ordinal);
        Assert.Contains("Harbor Study", sheet, StringComparison.Ordinal);
        Assert.Contains("pi1", sheet, StringComparison.Ordinal);
        Assert.Contains("MSFEA", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Matched projects", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Cumulative average", sheet, StringComparison.Ordinal);
        await profiles.DidNotReceive().ListByUserIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Admin_full_export_includes_account_fields_only()
    {
        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
            Arg.Any<string?>(),
            Arg.Any<UserRole?>(),
            Arg.Any<UserSortField>(),
            Arg.Any<SortDirection>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()).Returns([
            new User
            {
                Email = "admin@aub.edu.lb",
                Name = "Alex Admin",
                UserName = "aa100",
                Affiliation = "MSFEA",
                Role = UserRole.Admin,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);

        var profiles = Substitute.For<IStudentProfileRepository>();
        var projects = Substitute.For<IProjectRepository>();
        var file = await Handler(users, profiles, projects: projects).Handle(
            new ExportUsersQuery("xlsx", role: UserRole.Admin, detail: "full"),
            CancellationToken.None);

        Assert.Contains("urvp-users-full-admin-", file.FileName, StringComparison.Ordinal);
        var sheet = ReadSheet(file.Content);
        Assert.Contains("Username", sheet, StringComparison.Ordinal);
        Assert.Contains("aa100", sheet, StringComparison.Ordinal);
        Assert.Contains("MSFEA", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Posted projects", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("Matched projects", sheet, StringComparison.Ordinal);
        await profiles.DidNotReceive().ListByUserIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
        await projects.DidNotReceive().ListPostedTitlesByCreatorIdsAsync(
            Arg.Any<IReadOnlyCollection<Guid>>(),
            Arg.Any<CancellationToken>());
    }

    private static async Task<FEA.URVP.Application.DTOs.Users.UserExportFileDto> Handle(string format)
    {
        var handler = Handler(Users());
        return await handler.Handle(new ExportUsersQuery(format), CancellationToken.None);
    }

    private static ExportUsersQueryHandler Handler(
        IUserRepository users,
        IStudentProfileRepository? profiles = null,
        IMatchingRunRepository? runs = null,
        IProjectRepository? projects = null) =>
        new(
            users,
            profiles ?? Substitute.For<IStudentProfileRepository>(),
            runs ?? Substitute.For<IMatchingRunRepository>(),
            projects ?? Substitute.For<IProjectRepository>());

    private static IUserRepository Users()
    {
        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
            Arg.Any<string?>(),
            Arg.Any<UserRole?>(),
            Arg.Any<UserSortField>(),
            Arg.Any<SortDirection>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()).Returns([
            new User
            {
                Email = "aa624@mail.aub.edu",
                Name = "Ada",
                UserName = "aa624",
                Affiliation = "AUB",
                Role = UserRole.Student,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);
        return users;
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
