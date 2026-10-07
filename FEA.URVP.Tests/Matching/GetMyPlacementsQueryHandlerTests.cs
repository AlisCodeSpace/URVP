using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Matching.GetMine;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Matching;

public sealed class GetMyPlacementsQueryHandlerTests
{
    [Fact]
    public async Task Returns_confirmed_placements_the_student_did_not_rank()
    {
        var studentId = Guid.NewGuid();
        var student = Student(studentId);
        var ranked = ProjectFor("Ranked lab");
        var assigned = ProjectFor("Unranked lab");
        var assignedAt = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(studentId, Arg.Any<CancellationToken>()).Returns(student);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListCurrentConfirmedByStudentAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(
            [
                Placement(student, ranked, studentRank: 1, assignedAt),
                Placement(student, assigned, studentRank: 0, assignedAt),
            ]);

        var handler = Handler(runs, users);
        var placements = await handler.Handle(new GetMyPlacementsQuery(studentId), CancellationToken.None);

        Assert.Equal(2, placements.Count);
        var unranked = Assert.Single(placements, placement => placement.ProjectId == assigned.Id);
        Assert.Equal("Unranked lab", unranked.ProjectTitle);
        Assert.Equal("Dr. Faculty", unranked.FacultyName);
        Assert.Equal("FEA", unranked.FacultyAffiliation);
        Assert.Equal("Fall 2026", unranked.SemesterName);
        Assert.Equal(0, unranked.StudentRank);
        Assert.Equal(assignedAt, unranked.AssignedAt);
        Assert.Contains("Structures", unranked.ResearchAreas);
    }

    [Fact]
    public async Task Throws_when_the_caller_is_not_a_student()
    {
        var userId = Guid.NewGuid();
        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                Name = "Faculty",
                Email = "faculty@aub.edu.lb",
                UserName = "faculty",
                Affiliation = "FEA",
                Role = UserRole.Faculty,
            });

        var handler = Handler(Substitute.For<IMatchingRunRepository>(), users);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GetMyPlacementsQuery(userId), CancellationToken.None));
    }

    [Fact]
    public async Task Throws_when_the_user_id_is_empty()
    {
        var handler = Handler(
            Substitute.For<IMatchingRunRepository>(),
            Substitute.For<IUserRepository>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GetMyPlacementsQuery(Guid.Empty), CancellationToken.None));
    }

    private static GetMyPlacementsQueryHandler Handler(
        IMatchingRunRepository runs,
        IUserRepository users) =>
        new(runs, users, NullLogger<GetMyPlacementsQueryHandler>.Instance);

    private static User Student(Guid id) => new()
    {
        Id = id,
        Name = "Ada Lovelace",
        Email = "ada@aub.edu.lb",
        UserName = "ada",
        Affiliation = "FEA",
        Role = UserRole.Student,
    };

    private static Project ProjectFor(string title) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        BriefDescription = "Study",
        FacultyNameSnapshot = "Dr. Faculty",
        AffiliationSnapshot = "FEA",
        EmailSnapshot = "faculty@aub.edu.lb",
        ResearchAreas = ["Structures"],
        ActivityTypes = ["Lab"],
        Status = ProjectStatus.Open,
        Semester = new Semester { Name = "Fall 2026" },
    };

    private static Placement Placement(User student, Project project, byte studentRank, DateTime assignedAt) =>
        new()
        {
            ProjectId = project.Id,
            Project = project,
            StudentUserId = student.Id,
            StudentUser = student,
            StudentRank = studentRank,
            Status = PlacementStatus.Confirmed,
            CreatedAt = assignedAt,
            UpdatedAt = assignedAt,
        };
}
