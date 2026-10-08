using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Files.GetById;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Files;

public sealed class GetFileByIdQueryHandlerTests
{
    [Fact]
    public async Task Faculty_can_download_a_document_for_an_assigned_student()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var file = StudentCv(studentId);
        var (handler, rankings, placements) = Handler(facultyId, file);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        placements.StudentAssignedToFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(true);

        var content = await handler.Handle(
            new GetFileByIdQuery(file.Id, facultyId, isAdmin: false),
            CancellationToken.None);

        Assert.Equal("cv.pdf", content.FileName);
    }

    [Fact]
    public async Task Faculty_cannot_download_a_document_for_an_unrelated_student()
    {
        var facultyId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var file = StudentCv(studentId);
        var (handler, rankings, placements) = Handler(facultyId, file);
        rankings.StudentHasRankedFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);
        placements.StudentAssignedToFacultyProjectAsync(studentId, facultyId, Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetFileByIdQuery(file.Id, facultyId, isAdmin: false),
                CancellationToken.None));
    }

    private static (
        GetFileByIdQueryHandler Handler,
        IProjectRankingRepository Rankings,
        IMatchingRunRepository Placements) Handler(Guid facultyId, FileStorage file)
    {
        var files = Substitute.For<IFileStorageRepository>();
        files.FindByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(facultyId, Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = facultyId,
                Name = "Dr. Faculty",
                Email = "faculty@aub.edu.lb",
                UserName = "faculty",
                Affiliation = "FEA",
                Role = UserRole.Faculty,
            });

        var rankings = Substitute.For<IProjectRankingRepository>();
        var placements = Substitute.For<IMatchingRunRepository>();
        var alumni = Substitute.For<IProjectAlumniRepository>();
        alumni.StudentWasConfirmedOnFacultyProjectAsync(
                file.EntityId,
                facultyId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new GetFileByIdQueryHandler(files, users, rankings, placements, alumni);
        return (handler, rankings, placements);
    }

    private static FileStorage StudentCv(Guid studentId) =>
        new()
        {
            EntityType = FileStorageCatalog.EntityStudentProfile,
            EntityId = studentId,
            FileCategory = FileStorageCatalog.CategoryCv,
            FileName = "cv.pdf",
            MimeType = "application/pdf",
            FileSize = 4,
            ContentHash = [1, 2, 3, 4],
            Content = [1, 2, 3, 4],
        };
}
