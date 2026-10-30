using FEA.URVP.Application.Features.Projects.Commands.ResearchAssistants.Add;
using FEA.URVP.Application.Features.Projects.Commands.ResearchAssistants.Remove;
using FEA.URVP.Application.Features.Projects.Queries.ListAssistedProjects;
using FEA.URVP.Application.Features.Projects.Queries.ListResearchAssistants;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FEA.URVP.Tests.Projects;

public sealed class ProjectResearchAssistantTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AddProjectResearchAssistantCommandHandler _add;
    private readonly RemoveProjectResearchAssistantCommandHandler _remove;
    private readonly ListProjectResearchAssistantsQueryHandler _list;
    private readonly ListAssistedProjectsQueryHandler _assisting;
    private readonly User _faculty;
    private readonly User _admin;
    private readonly Project _project;

    public ProjectResearchAssistantTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var semester = new Semester { Name = "Fall 2026", IsActive = true };
        _faculty = Person("Ada Faculty", "ada.faculty@mail.aub.edu", UserRole.Faculty);
        _admin = Person("Admin", "admin@mail.aub.edu", UserRole.Admin);
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
            SemesterId = semester.Id,
            CreatedByUserId = _faculty.Id,
        };

        _db.Semesters.Add(semester);
        _db.Users.AddRange(_faculty, _admin);
        _db.Projects.Add(_project);
        _db.SaveChanges();

        var projects = new ProjectRepository(_db);
        var assistants = new ProjectResearchAssistantRepository(_db);
        _add = new AddProjectResearchAssistantCommandHandler(
            NullLogger<AddProjectResearchAssistantCommandHandler>.Instance,
            _db,
            projects,
            assistants);
        _remove = new RemoveProjectResearchAssistantCommandHandler(
            NullLogger<RemoveProjectResearchAssistantCommandHandler>.Instance,
            _db,
            projects,
            assistants);
        _list = new ListProjectResearchAssistantsQueryHandler(projects, assistants);
        _assisting = new ListAssistedProjectsQueryHandler(new UserRepository(_db), assistants);
    }

    [Fact]
    public async Task Email_is_stored_lowercase_and_has_no_name_until_the_person_signs_in()
    {
        var added = await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            Email = "  Lab.Aide@Mail.AUB.edu ",
        }, CancellationToken.None);

        Assert.Equal("lab.aide@mail.aub.edu", added.Email);
        Assert.Null(added.Name);
        Assert.False(added.HasSignedIn);

        _db.Users.Add(Person("Lab Aide", "lab.aide@mail.aub.edu", UserRole.Student));
        await _db.SaveChangesAsync();

        var listed = await _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, _faculty.Id, false),
            CancellationToken.None);

        var entry = Assert.Single(listed);
        Assert.Equal("Lab Aide", entry.Name);
        Assert.True(entry.HasSignedIn);
    }

    [Fact]
    public async Task Person_added_before_their_first_sign_in_sees_the_project_afterwards()
    {
        await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _admin.Id,
            IsAdmin = true,
            Email = "new.person@mail.aub.edu",
        }, CancellationToken.None);

        var newcomer = Person("New Person", "new.person@mail.aub.edu", UserRole.Student);
        var bystander = Person("Someone Else", "else@mail.aub.edu", UserRole.Student);
        _db.Users.AddRange(newcomer, bystander);
        await _db.SaveChangesAsync();

        var mine = await _assisting.Handle(new ListAssistedProjectsQuery(newcomer.Id), CancellationToken.None);
        var theirs = await _assisting.Handle(new ListAssistedProjectsQuery(bystander.Id), CancellationToken.None);

        var project = Assert.Single(mine);
        Assert.Equal(_project.Id, project.ProjectId);
        Assert.Equal("Water systems", project.Title);
        Assert.Equal("Fall 2026", project.SemesterName);
        Assert.Empty(theirs);
    }

    [Fact]
    public async Task Owner_can_remove_an_assistant()
    {
        var added = await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        await _remove.Handle(
            new RemoveProjectResearchAssistantCommand(_project.Id, added.Id, _faculty.Id, false),
            CancellationToken.None);

        var after = await _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, _admin.Id, true),
            CancellationToken.None);
        Assert.Empty(after);
    }

    [Fact]
    public async Task Admin_can_add_on_another_faculty_project()
    {
        await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _admin.Id,
            IsAdmin = true,
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        Assert.Equal(_admin.Id, _db.ProjectResearchAssistants.Single().AddedByUserId);
    }

    [Fact]
    public async Task Other_faculty_cannot_add_or_list()
    {
        var other = Person("Other Faculty", "other@mail.aub.edu", UserRole.Faculty);
        _db.Users.Add(other);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _add.Handle(
            new AddProjectResearchAssistantCommand
            {
                ProjectId = _project.Id,
                CurrentUserId = other.Id,
                Email = "aide@mail.aub.edu",
            },
            CancellationToken.None));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, other.Id, false),
            CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_a_duplicate_email_regardless_of_case()
    {
        await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        var duplicate = await Assert.ThrowsAsync<InvalidOperationException>(() => _add.Handle(
            new AddProjectResearchAssistantCommand
            {
                ProjectId = _project.Id,
                CurrentUserId = _faculty.Id,
                Email = "AIDE@mail.aub.edu",
            },
            CancellationToken.None));

        Assert.Contains("already a research assistant", duplicate.Message);
    }

    [Fact]
    public async Task Rejects_the_project_owner_as_their_own_assistant()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => _add.Handle(
            new AddProjectResearchAssistantCommand
            {
                ProjectId = _project.Id,
                CurrentUserId = _admin.Id,
                IsAdmin = true,
                Email = "ADA.FACULTY@mail.aub.edu",
            },
            CancellationToken.None));

        Assert.Contains("cannot also be its research assistant", error.Message);
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
