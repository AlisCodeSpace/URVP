using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Projects;
using FEA.URVP.Application.Queries.Projects.GetById;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Projects;

public sealed class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async Task Student_can_open_a_project_they_occupy_after_the_application_window_closes()
    {
        var (handler, runs, project, semester) = await ClosedWindowAsync();
        runs.StudentOccupiesProjectAsync(Arg.Any<Guid>(), project.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await handler.Handle(
            new GetProjectByIdQuery(project.Id, Guid.NewGuid(), ViewerIsAdmin: false, ViewerIsStudent: true),
            CancellationToken.None);

        Assert.Equal(project.Title, result.Title);
        Assert.Equal(semester.Name, result.SemesterName);
    }

    [Fact]
    public async Task Student_cannot_open_other_projects_after_the_application_window_closes()
    {
        var (handler, runs, project, _) = await ClosedWindowAsync();
        runs.StudentOccupiesProjectAsync(Arg.Any<Guid>(), project.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new GetProjectByIdQuery(project.Id, Guid.NewGuid(), ViewerIsAdmin: false, ViewerIsStudent: true),
                CancellationToken.None));

        Assert.Equal(
            "Projects are visible to students only while the application window is open.",
            error.Message);
    }

    private static async Task<(
        GetProjectByIdQueryHandler Handler,
        IMatchingRunRepository Runs,
        Project Project,
        Semester Semester)> ClosedWindowAsync()
    {
        var now = DateTime.UtcNow;
        var semester = new Semester
        {
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = now.AddDays(-30),
            CycleEnd = now.AddDays(60),
            ApplicationWindowStart = now.AddDays(-20),
            ApplicationWindowEnd = now.AddDays(-1),
            RegistrationWindowStart = now.AddDays(-25),
            RegistrationWindowEnd = now.AddDays(-2),
        };
        var project = new Project
        {
            SemesterId = semester.Id,
            Semester = semester,
            Title = "Bridge sensors",
            BriefDescription = "Study",
            FacultyNameSnapshot = "Dr. Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@aub.edu.lb",
            ResearchAreas = ["Structures"],
            ActivityTypes = ["Lab"],
            Status = ProjectStatus.Open,
            VolunteersRequired = 2,
        };

        var projects = Substitute.For<IProjectRepository>();
        projects.FindByIdAsync(project.Id, Arg.Any<CancellationToken>()).Returns(project);
        projects.ListTrackedOnEndedCyclesAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Project>());

        var semesters = Substitute.For<ISemesterRepository>();
        semesters.FindActiveAsync(Arg.Any<CancellationToken>()).Returns(semester);

        var runs = Substitute.For<IMatchingRunRepository>();
        var rankings = Substitute.For<IProjectRankingRepository>();
        var closure = new ProjectCycleClosure(
            projects,
            rankings,
            Substitute.For<IFacultyCandidateRankingRepository>(),
            runs,
            Substitute.For<IProjectAlumniRepository>(),
            Substitute.For<IUnitOfWork>());
        var access = new FacultyProjectMutationAccess(semesters, rankings);
        var handler = new GetProjectByIdQueryHandler(projects, semesters, access, closure, runs);

        await Task.CompletedTask;
        return (handler, runs, project, semester);
    }
}
