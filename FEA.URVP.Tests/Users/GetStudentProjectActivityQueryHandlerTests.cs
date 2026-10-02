using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Users.GetProjectActivity;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Users;

public sealed class GetStudentProjectActivityQueryHandlerTests
{
    [Fact]
    public async Task Returns_current_cycle_rankings_and_confirmed_assignments()
    {
        var studentId = Guid.NewGuid();
        var semesterId = Guid.NewGuid();
        var student = new User
        {
            Id = studentId,
            Name = "Ada Lovelace",
            Email = "ada@aub.edu.lb",
            UserName = "ada",
            Affiliation = "FEA",
            Role = UserRole.Student,
        };
        var current = ProjectFor(semesterId, "Bridge sensors", "Dr. Current");
        var previous = ProjectFor(Guid.NewGuid(), "Old survey", "Dr. Previous");
        var rankedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var assignedAt = new DateTime(2026, 9, 12, 0, 0, 0, DateTimeKind.Utc);

        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(studentId, Arg.Any<CancellationToken>()).Returns(student);

        var semesters = Substitute.For<ISemesterRepository>();
        semesters.FindActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new Semester { Id = semesterId, Name = "Fall 2026", IsActive = true });

        var rankings = Substitute.For<IProjectRankingRepository>();
        rankings.ListByStudentAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(
            [
                Ranking(studentId, previous, 2, rankedAt),
                Ranking(studentId, current, 1, rankedAt),
            ]);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedProjectIdsByStudentAsync(studentId, semesterId, Arg.Any<CancellationToken>())
            .Returns([current.Id]);
        runs.ListConfirmedByStudentIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new Placement
                {
                    ProjectId = current.Id,
                    Project = current,
                    StudentUserId = studentId,
                    StudentUser = student,
                    StudentRank = 1,
                    FacultyRank = 2,
                    Source = PlacementSource.Manual,
                    Status = PlacementStatus.Confirmed,
                    CreatedAt = assignedAt,
                    UpdatedAt = assignedAt,
                },
            ]);

        var handler = new GetStudentProjectActivityQueryHandler(users, rankings, runs, semesters);
        var activity = await handler.Handle(new GetStudentProjectActivityQuery(studentId), CancellationToken.None);

        var ranking = Assert.Single(activity.Rankings);
        Assert.Equal(current.Id, ranking.ProjectId);
        Assert.Equal("Bridge sensors", ranking.ProjectTitle);
        Assert.Equal(1, ranking.Rank);
        Assert.True(ranking.IsMatched);

        var assignment = Assert.Single(activity.Assignments);
        Assert.Equal(current.Id, assignment.ProjectId);
        Assert.Equal("Bridge sensors", assignment.ProjectTitle);
        Assert.Equal("Ada Lovelace", assignment.StudentName);
        Assert.Equal(PlacementSource.Manual, assignment.Source);
    }

    [Fact]
    public async Task Throws_when_the_user_does_not_exist()
    {
        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var handler = new GetStudentProjectActivityQueryHandler(
            users,
            Substitute.For<IProjectRankingRepository>(),
            Substitute.For<IMatchingRunRepository>(),
            Substitute.For<ISemesterRepository>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetStudentProjectActivityQuery(Guid.NewGuid()), CancellationToken.None));
    }

    private static Project ProjectFor(Guid semesterId, string title, string facultyName) => new()
    {
        Id = Guid.NewGuid(),
        SemesterId = semesterId,
        Title = title,
        BriefDescription = "Study",
        FacultyNameSnapshot = facultyName,
        AffiliationSnapshot = "FEA",
        EmailSnapshot = "faculty@aub.edu.lb",
        ResearchAreas = ["Structures"],
        Status = ProjectStatus.Open,
    };

    private static ProjectRanking Ranking(Guid studentId, Project project, byte rank, DateTime rankedAt) => new()
    {
        StudentUserId = studentId,
        ProjectId = project.Id,
        Project = project,
        Rank = rank,
        CreatedAt = rankedAt,
        UpdatedAt = rankedAt,
    };
}
