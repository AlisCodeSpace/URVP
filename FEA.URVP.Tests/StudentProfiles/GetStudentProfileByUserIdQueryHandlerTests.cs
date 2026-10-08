using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.StudentProfiles.GetByUserId;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.StudentProfiles;

public sealed class GetStudentProfileByUserIdQueryHandlerTests
{
    [Fact]
    public async Task Faculty_can_view_a_student_assigned_without_ranking()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var (handler, rankings, placements, alumni) = Handler(facultyId, studentId);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        placements.StudentAssignedToFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(true);

        var profile = await handler.Handle(
            new GetStudentProfileByUserIdQuery(facultyId, studentId),
            CancellationToken.None);

        Assert.Equal(studentId, profile.UserId);
        Assert.Equal("Ada", profile.FirstName);
        await alumni.DidNotReceive()
            .StudentWasConfirmedOnFacultyProjectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Faculty_can_view_a_student_assigned_on_an_earlier_cycle()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var (handler, rankings, placements, _) = Handler(facultyId, studentId);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        placements.StudentAssignedToFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);

        var profile = await handler.Handle(
            new GetStudentProfileByUserIdQuery(facultyId, studentId),
            CancellationToken.None);

        Assert.Equal("Lovelace", profile.LastName);
    }

    [Fact]
    public async Task Faculty_can_still_view_a_student_who_ranked_their_project()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var (handler, rankings, placements, _) = Handler(facultyId, studentId);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(true);

        var profile = await handler.Handle(
            new GetStudentProfileByUserIdQuery(facultyId, studentId),
            CancellationToken.None);

        Assert.Equal(studentId, profile.UserId);
        await placements.DidNotReceive()
            .StudentAssignedToFacultyProjectAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Faculty_cannot_view_an_unrelated_student()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var (handler, rankings, placements, alumni) = Handler(facultyId, studentId);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        placements.StudentAssignedToFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        alumni.StudentWasConfirmedOnFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetStudentProfileByUserIdQuery(facultyId, studentId),
                CancellationToken.None));

        Assert.Equal(
            "You can only view profiles of students who ranked or were assigned to a project you posted.",
            error.Message);
    }

    private static (
        GetStudentProfileByUserIdQueryHandler Handler,
        IProjectRankingRepository Rankings,
        IMatchingRunRepository Placements,
        IProjectAlumniRepository Alumni) Handler(Guid facultyId, Guid studentId)
    {
        var faculty = Person(facultyId, "Dr. Faculty", "faculty@aub.edu.lb", UserRole.Faculty);
        var student = Person(studentId, "Ada Lovelace", "ada@mail.aub.edu.lb", UserRole.Student);

        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(facultyId, Arg.Any<CancellationToken>()).Returns(faculty);
        users.FindByIdAsync(studentId, Arg.Any<CancellationToken>()).Returns(student);

        var rankings = Substitute.For<IProjectRankingRepository>();
        var placements = Substitute.For<IMatchingRunRepository>();
        var alumni = Substitute.For<IProjectAlumniRepository>();
        alumni.StudentWasConfirmedOnFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new GetStudentProfileByUserIdQueryHandler(
            Substitute.For<IStudentProfileRepository>(),
            users,
            Substitute.For<IFileStorageRepository>(),
            rankings,
            placements,
            alumni);

        return (handler, rankings, placements, alumni);
    }

    private static User Person(Guid id, string name, string email, UserRole role) =>
        new()
        {
            Id = id,
            Name = name,
            Email = email,
            UserName = email,
            Affiliation = "FEA",
            Role = role,
        };
}
