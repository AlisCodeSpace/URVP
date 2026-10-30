using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Features.FacultyCandidateRankings.Commands.Upsert;
using FEA.URVP.Application.Features.ProjectRankings.Queries.ListByProject;
using FEA.URVP.Application.Features.Projects;
using FEA.URVP.Application.Features.Projects.Commands.Update;
using FEA.URVP.Application.Features.Projects.Queries.GetById;
using FEA.URVP.Application.Features.StudentProfiles;
using FEA.URVP.Application.Features.StudentProfiles.Queries.GetByUserId;
using FEA.URVP.Domain.Entities.ProjectRankings;
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

namespace FEA.URVP.Tests.Projects;

public sealed class ResearchAssistantReviewTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly User _faculty;
    private readonly User _assistant;
    private readonly User _bystander;
    private readonly User _student;
    private readonly Project _project;
    private readonly Semester _semester;
    private readonly UpsertFacultyCandidateRankingCommandHandler _rank;
    private readonly ListProjectRankingsQueryHandler _listRankings;
    private readonly GetProjectByIdQueryHandler _getProject;
    private readonly GetStudentProfileByUserIdQueryHandler _profile;
    private readonly UpdateProjectCommandHandler _update;

    public ResearchAssistantReviewTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new AppDbContext(options);

        var now = DateTime.UtcNow;
        _semester = new Semester
        {
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = now.AddDays(-10),
            CycleEnd = now.AddDays(80),
            ApplicationWindowStart = now.AddDays(-2),
            ApplicationWindowEnd = now.AddDays(20),
        };
        _faculty = Person("Ada Faculty", "ada.faculty@mail.aub.edu", UserRole.Faculty);
        _assistant = Person("Lab Aide", "lab.aide@mail.aub.edu", UserRole.Student);
        _bystander = Person("Someone Else", "else@mail.aub.edu", UserRole.Student);
        _student = Person("Pat Student", "pat.student@mail.aub.edu", UserRole.Student);
        _project = new Project
        {
            Title = "Water systems",
            ResearchAreas = ["Water"],
            ActivityTypes = ["Lab"],
            FacultyNameSnapshot = _faculty.Name,
            AffiliationSnapshot = "FEA",
            EmailSnapshot = _faculty.Email,
            BriefDescription = "Research",
            VolunteersRequired = 2,
            SemesterId = _semester.Id,
            CreatedByUserId = _faculty.Id,
            Status = ProjectStatus.Open,
        };

        _db.Semesters.Add(_semester);
        _db.Users.AddRange(_faculty, _assistant, _bystander, _student);
        _db.Projects.Add(_project);
        _db.ProjectResearchAssistants.Add(new ProjectResearchAssistant
        {
            ProjectId = _project.Id,
            Email = _assistant.Email,
            AddedByUserId = _faculty.Id,
        });
        _db.ProjectRankings.Add(new ProjectRanking
        {
            ProjectId = _project.Id,
            StudentUserId = _student.Id,
            Rank = 1,
        });
        _db.SaveChanges();

        var projects = new ProjectRepository(_db);
        var users = new UserRepository(_db);
        var assistants = new ProjectResearchAssistantRepository(_db);
        var rankings = new ProjectRankingRepository(_db);
        var candidateRankings = new FacultyCandidateRankingRepository(_db);
        var semesters = new SemesterRepository(_db);
        var runs = new MatchingRunRepository(_db);
        var alumni = new ProjectAlumniRepository(_db);
        var closure = new ProjectCycleClosure(projects, rankings, candidateRankings, runs, alumni, _db);
        var access = new FacultyProjectMutationAccess(semesters, rankings);
        var reviewAccess = new ProjectResearchAssistantAccess(users, assistants);

        _rank = new UpsertFacultyCandidateRankingCommandHandler(
            NullLogger<UpsertFacultyCandidateRankingCommandHandler>.Instance,
            _db,
            candidateRankings,
            rankings,
            projects,
            users,
            semesters,
            reviewAccess);
        _listRankings = new ListProjectRankingsQueryHandler(
            projects, rankings, candidateRankings, reviewAccess);
        _getProject = new GetProjectByIdQueryHandler(
            projects, semesters, access, closure, runs, reviewAccess);
        _profile = new GetStudentProfileByUserIdQueryHandler(
            new StudentProfileRepository(_db),
            users,
            new FileStorageRepository(_db),
            new StudentProfileViewAccess(rankings, runs, alumni, assistants));
        _update = new UpdateProjectCommandHandler(
            NullLogger<UpdateProjectCommandHandler>.Instance,
            _db,
            projects,
            users,
            Substitute.For<IEventBus>(),
            access,
            closure);
    }

    [Fact]
    public async Task Assistant_can_view_rankings_and_rank_an_applicant()
    {
        var before = await _listRankings.Handle(
            new ListProjectRankingsQuery(_project.Id, _assistant.Id, false),
            CancellationToken.None);
        var applicant = Assert.Single(before);
        Assert.Equal(_student.Id, applicant.StudentUserId);
        Assert.Null(applicant.FacultyRank);

        var saved = await _rank.Handle(new UpsertFacultyCandidateRankingCommand
        {
            ProjectId = _project.Id,
            StudentUserId = _student.Id,
            Rank = 1,
            CurrentUserId = _assistant.Id,
        }, CancellationToken.None);

        Assert.Equal((byte)1, saved.Rank);
        Assert.Equal(_assistant.Id, saved.RankedByUserId);

        var stored = await _db.FacultyCandidateRankings.SingleAsync();
        Assert.Equal(_assistant.Id, stored.RankedByUserId);

        var after = await _listRankings.Handle(
            new ListProjectRankingsQuery(_project.Id, _assistant.Id, false),
            CancellationToken.None);
        var ranked = Assert.Single(after);
        Assert.Equal((byte)1, ranked.FacultyRank);
        Assert.Equal("Lab Aide", ranked.RankedByName);
    }

    [Fact]
    public async Task Someone_else_cannot_view_or_rank()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _listRankings.Handle(
            new ListProjectRankingsQuery(_project.Id, _bystander.Id, false),
            CancellationToken.None));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _rank.Handle(
            new UpsertFacultyCandidateRankingCommand
            {
                ProjectId = _project.Id,
                StudentUserId = _student.Id,
                Rank = 1,
                CurrentUserId = _bystander.Id,
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task Assistant_who_applied_cannot_rank_themselves()
    {
        _db.ProjectRankings.Add(new ProjectRanking
        {
            ProjectId = _project.Id,
            StudentUserId = _assistant.Id,
            Rank = 2,
        });
        _db.SaveChanges();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _rank.Handle(
            new UpsertFacultyCandidateRankingCommand
            {
                ProjectId = _project.Id,
                StudentUserId = _assistant.Id,
                Rank = 1,
                CurrentUserId = _assistant.Id,
            },
            CancellationToken.None));

        Assert.False(await _db.FacultyCandidateRankings.AnyAsync());
    }

    [Fact]
    public async Task Assistant_can_open_the_project_after_applications_close()
    {
        CloseApplications();

        var project = await _getProject.Handle(
            new GetProjectByIdQuery(_project.Id, _assistant.Id, ViewerIsAdmin: false, ViewerIsStudent: true),
            CancellationToken.None);

        Assert.Equal(_project.Title, project.Title);
    }

    [Fact]
    public async Task Other_student_cannot_open_the_project_after_applications_close()
    {
        CloseApplications();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => _getProject.Handle(
            new GetProjectByIdQuery(_project.Id, _bystander.Id, ViewerIsAdmin: false, ViewerIsStudent: true),
            CancellationToken.None));

        Assert.Equal(
            "Projects are visible to students only while the application window is open.",
            error.Message);
    }

    [Fact]
    public async Task Assistant_can_open_an_applicant_profile()
    {
        var profile = await _profile.Handle(
            new GetStudentProfileByUserIdQuery(_assistant.Id, _student.Id),
            CancellationToken.None);

        Assert.Equal(_student.Id, profile.UserId);
        Assert.Equal("Pat", profile.FirstName);
    }

    [Fact]
    public async Task Someone_else_cannot_open_the_applicant_profile()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _profile.Handle(
            new GetStudentProfileByUserIdQuery(_bystander.Id, _student.Id),
            CancellationToken.None));
    }

    [Fact]
    public async Task Assistant_cannot_edit_the_project()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _update.Handle(new UpdateProjectCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _assistant.Id,
            Title = "Renamed",
            ResearchAreas = ["Water"],
            BriefDescription = "Research",
            ActivityTypes = ["Lab"],
            VolunteersRequired = 2,
            Status = ProjectStatus.Open,
        }, CancellationToken.None));

        var stored = await _db.Projects.SingleAsync(project => project.Id == _project.Id);
        Assert.Equal("Water systems", stored.Title);
    }

    private void CloseApplications()
    {
        _semester.ApplicationWindowEnd = DateTime.UtcNow.AddMinutes(-1);
        _db.SaveChanges();
    }

    private static User Person(string name, string email, UserRole role) => new()
    {
        Name = name,
        Email = email,
        UserName = email.Split('@')[0],
        Affiliation = "AUB",
        Role = role,
    };

    public void Dispose() => _db.Dispose();
}
