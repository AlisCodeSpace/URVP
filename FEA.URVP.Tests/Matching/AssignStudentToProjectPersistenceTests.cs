using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Matching.Assign;
using FEA.URVP.Application.Projects;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Matching;

public sealed class AssignStudentToProjectPersistenceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AssignStudentToProjectCommandHandler _handler;

    public AssignStudentToProjectPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings =>
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _db = new AppDbContext(options);

        var projects = new ProjectRepository(_db);
        var runs = new MatchingRunRepository(_db);
        _handler = new AssignStudentToProjectCommandHandler(
            NullLogger<AssignStudentToProjectCommandHandler>.Instance,
            _db,
            projects,
            new UserRepository(_db),
            new SemesterRepository(_db),
            runs,
            new ProjectRankingRepository(_db),
            new FacultyCandidateRankingRepository(_db),
            Substitute.For<IEventBus>(),
            new ProjectCycleClosure(
                projects,
                new ProjectRankingRepository(_db),
                new FacultyCandidateRankingRepository(_db),
                runs,
                new ProjectAlumniRepository(_db),
                _db));
    }

    [Fact]
    public async Task Assigns_a_second_student_on_the_existing_manual_run()
    {
        var now = DateTime.UtcNow;
        var faculty = User("faculty@mail.aub.edu", "Faculty", UserRole.Faculty);
        var first = User("one@mail.aub.edu", "One", UserRole.Student);
        var second = User("two@mail.aub.edu", "Two", UserRole.Student);
        var semester = new Semester
        {
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = now.AddDays(-20),
            CycleEnd = now.AddDays(80),
            ApplicationWindowStart = now.AddDays(-10),
            ApplicationWindowEnd = now.AddDays(-1),
        };
        var project = new Project
        {
            CreatedByUserId = faculty.Id,
            SemesterId = semester.Id,
            Title = "Water systems",
            BriefDescription = "Research",
            VolunteersRequired = 1,
            Status = ProjectStatus.Open,
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = faculty.Email,
            ResearchAreas = ["Water"],
            ActivityTypes = ["Lab"],
        };

        _db.Users.AddRange(faculty, first, second);
        _db.Semesters.Add(semester);
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await Assign(project.Id, first.Id);
        await Assign(project.Id, second.Id);

        var placements = await _db.Placements.AsNoTracking().ToListAsync();
        Assert.Equal(2, placements.Count);
        Assert.All(placements, placement =>
        {
            Assert.Equal(project.Id, placement.ProjectId);
            Assert.Equal(PlacementStatus.Confirmed, placement.Status);
            Assert.Equal(PlacementSource.Manual, placement.Source);
        });
        Assert.Contains(placements, placement => placement.StudentUserId == first.Id);
        Assert.Contains(placements, placement => placement.StudentUserId == second.Id);

        var storedProject = await _db.Projects.AsNoTracking().SingleAsync(p => p.Id == project.Id);
        Assert.Equal(1, storedProject.VolunteersRequired);
        Assert.Equal(2, storedProject.VolunteersFilled);
        Assert.Equal(ProjectStatus.Matching, storedProject.Status);
    }

    private Task Assign(Guid projectId, Guid studentUserId) =>
        _handler.Handle(
            new AssignStudentToProjectCommand
            {
                CurrentUserId = Guid.NewGuid(),
                ProjectId = projectId,
                StudentUserId = studentUserId,
            },
            CancellationToken.None);

    private static User User(string email, string name, UserRole role) => new()
    {
        Email = email,
        Name = name,
        UserName = email.Split('@')[0],
        Affiliation = "FEA",
        Role = role,
    };

    public void Dispose() => _db.Dispose();
}
