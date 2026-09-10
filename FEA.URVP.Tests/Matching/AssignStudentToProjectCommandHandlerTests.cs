using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Matching.Assign;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events;
using FEA.URVP.Domain.Events.Matching;
using FEA.URVP.Tests.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Matching;

public sealed class AssignStudentToProjectCommandHandlerTests
{
    [Fact]
    public async Task Assigns_student_and_publishes_event()
    {
        var (handler, bus, runs, project, student) = CreateHandler();
        MatchingRun? saved = null;
        runs.When(x => x.Add(Arg.Any<MatchingRun>()))
            .Do(call => saved = call.Arg<MatchingRun>());

        var dto = await handler.Handle(
            new AssignStudentToProjectCommand
            {
                CurrentUserId = Guid.NewGuid(),
                ProjectId = project.Id,
                StudentUserId = student.Id,
            },
            CancellationToken.None);

        Assert.Equal(student.Id, dto.StudentUserId);
        Assert.Equal(project.Id, dto.ProjectId);
        Assert.Equal(PlacementStatus.Confirmed, dto.Status);
        Assert.Equal(PlacementSource.Manual, dto.Source);
        Assert.NotNull(saved);
        Assert.Equal(MatchingRun.ManualAlgorithmVersion, saved.AlgorithmVersion);
        Assert.Equal(MatchingRunStatus.Confirmed, saved.Status);
        Assert.Single(bus.Events.OfType<PlacementAssignedEvent>());
    }

    [Fact]
    public async Task Rejects_when_project_is_full()
    {
        var (handler, _, runs, project, student) = CreateHandler();
        project.VolunteersRequired = 1;
        runs.CountConfirmedByProjectAsync(project.Id, Arg.Any<CancellationToken>()).Returns(1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AssignStudentToProjectCommand
                {
                    CurrentUserId = Guid.NewGuid(),
                    ProjectId = project.Id,
                    StudentUserId = student.Id,
                },
                CancellationToken.None));

        Assert.Contains("full", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Rejects_when_student_is_already_on_another_project()
    {
        var student = Student();
        var project = OpenProject();
        var other = new Project
        {
            Title = "Other project",
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "f@mail.aub.edu",
            BriefDescription = "Research",
            VolunteersRequired = 2,
        };

        var (handler, _, _, _, _) = CreateHandler(
            student: student,
            project: project,
            configureProjects: projects =>
            {
                projects.FindByIdAsync(project.Id, Arg.Any<CancellationToken>()).Returns(project);
                projects.FindByIdAsync(other.Id, Arg.Any<CancellationToken>()).Returns(other);
            },
            configureRuns: runs =>
            {
                runs.ListConfirmedProjectIdsByStudentAsync(student.Id, Arg.Any<CancellationToken>())
                    .Returns([other.Id]);
            });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AssignStudentToProjectCommand
                {
                    CurrentUserId = Guid.NewGuid(),
                    ProjectId = project.Id,
                    StudentUserId = student.Id,
                },
                CancellationToken.None));

        Assert.Contains("Other project", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Revives_cancelled_placement_on_the_manual_run()
    {
        var student = Student();
        var project = OpenProject();
        var existing = new Placement
        {
            ProjectId = Guid.NewGuid(),
            StudentUserId = student.Id,
            Status = PlacementStatus.Cancelled,
            Source = PlacementSource.Manual,
        };
        var run = new MatchingRun
        {
            SemesterId = Guid.NewGuid(),
            AlgorithmVersion = MatchingRun.ManualAlgorithmVersion,
            Status = MatchingRunStatus.Confirmed,
            Seed = 0,
            Placements = [existing],
        };
        existing.MatchingRunId = run.Id;

        var (handler, bus, _, _, _) = CreateHandler(
            student: student,
            project: project,
            configureRuns: runs =>
            {
                runs.FindManualBySemesterAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                    .Returns(run);
                runs.ListConfirmedProjectIdsByStudentAsync(student.Id, Arg.Any<CancellationToken>())
                    .Returns(Array.Empty<Guid>());
                runs.CountConfirmedByProjectAsync(project.Id, Arg.Any<CancellationToken>())
                    .Returns(0, 1);
            });

        var dto = await handler.Handle(
            new AssignStudentToProjectCommand
            {
                CurrentUserId = Guid.NewGuid(),
                ProjectId = project.Id,
                StudentUserId = student.Id,
            },
            CancellationToken.None);

        Assert.Equal(PlacementStatus.Confirmed, existing.Status);
        Assert.Equal(project.Id, existing.ProjectId);
        Assert.Equal(dto.Id, existing.Id);
        Assert.Single(bus.Events.OfType<PlacementAssignedEvent>());
    }

    [Fact]
    public async Task Rejects_when_application_window_is_open()
    {
        var now = DateTime.UtcNow;
        var (handler, _, _, project, student) = CreateHandler(semester: new Semester
        {
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = now.AddDays(-20),
            CycleEnd = now.AddDays(80),
            ApplicationWindowStart = now.AddDays(-5),
            ApplicationWindowEnd = now.AddDays(5),
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AssignStudentToProjectCommand
                {
                    CurrentUserId = Guid.NewGuid(),
                    ProjectId = project.Id,
                    StudentUserId = student.Id,
                },
                CancellationToken.None));

        Assert.Contains("application window", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static (
        AssignStudentToProjectCommandHandler Handler,
        CapturingEventBus Bus,
        IMatchingRunRepository Runs,
        Project Project,
        User Student)
        CreateHandler(
            User? student = null,
            Project? project = null,
            Semester? semester = null,
            Action<IProjectRepository>? configureProjects = null,
            Action<IMatchingRunRepository>? configureRuns = null)
    {
        student ??= Student();
        project ??= OpenProject();
        semester ??= new Semester { Name = "Fall 2026" };
        var bus = new CapturingEventBus();

        var projects = Substitute.For<IProjectRepository>();
        projects.FindByIdAsync(project.Id, Arg.Any<CancellationToken>()).Returns(project);
        configureProjects?.Invoke(projects);

        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(student.Id, Arg.Any<CancellationToken>()).Returns(student);

        var semesters = Substitute.For<ISemesterRepository>();
        semesters.FindActiveAsync(Arg.Any<CancellationToken>()).Returns(semester);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedProjectIdsByStudentAsync(student.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Guid>());
        runs.CountConfirmedByProjectAsync(project.Id, Arg.Any<CancellationToken>()).Returns(0, 1);
        runs.FindManualBySemesterAsync(semester.Id, Arg.Any<CancellationToken>())
            .Returns((MatchingRun?)null);
        configureRuns?.Invoke(runs);

        var studentRankings = Substitute.For<IProjectRankingRepository>();
        var facultyRankings = Substitute.For<IFacultyCandidateRankingRepository>();

        var handler = new AssignStudentToProjectCommandHandler(
            NullLogger<AssignStudentToProjectCommandHandler>.Instance,
            new ImmediateUnitOfWork(),
            projects,
            users,
            semesters,
            runs,
            studentRankings,
            facultyRankings,
            bus);

        return (handler, bus, runs, project, student);
    }

    private static User Student() => new()
    {
        Email = "student@mail.aub.edu",
        Name = "Student",
        UserName = "student",
        Affiliation = "FEA",
        Role = UserRole.Student,
    };

    private static Project OpenProject() => new()
    {
        Title = "Water systems",
        FacultyNameSnapshot = "Faculty",
        AffiliationSnapshot = "FEA",
        EmailSnapshot = "faculty@mail.aub.edu",
        BriefDescription = "Research",
        VolunteersRequired = 2,
        Status = ProjectStatus.Open,
    };

    private sealed class CapturingEventBus : IEventBus
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
            where T : IDomainEvent
        {
            Events.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task PublishAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
        {
            Events.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }
}
