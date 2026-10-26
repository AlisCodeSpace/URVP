using FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;
using FEA.URVP.Application.Commands.Projects.ResearchAssistants.Remove;
using FEA.URVP.Application.Queries.Projects.ListResearchAssistants;
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
        var users = new UserRepository(_db);
        var assistants = new ProjectResearchAssistantRepository(_db);
        _add = new AddProjectResearchAssistantCommandHandler(
            NullLogger<AddProjectResearchAssistantCommandHandler>.Instance,
            _db,
            projects,
            users,
            assistants);
        _remove = new RemoveProjectResearchAssistantCommandHandler(
            NullLogger<RemoveProjectResearchAssistantCommandHandler>.Instance,
            _db,
            projects,
            assistants);
        _list = new ListProjectResearchAssistantsQueryHandler(projects, assistants);
    }

    [Fact]
    public async Task Owner_can_add_a_directory_user_and_a_typed_entry_then_remove_one()
    {
        var student = Person("Grace Hopper", "grace@mail.aub.edu", UserRole.Student);
        _db.Users.Add(student);
        await _db.SaveChangesAsync();

        var linked = await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            UserId = student.Id,
        }, CancellationToken.None);

        var typed = await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            Name = "Lab Aide",
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        var listed = await _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, _faculty.Id, false),
            CancellationToken.None);

        Assert.Equal(student.Id, linked.UserId);
        Assert.Equal("Grace Hopper", linked.Name);
        Assert.Null(typed.UserId);
        Assert.Equal(["Grace Hopper", "Lab Aide"], listed.Select(assistant => assistant.Name).ToArray());

        await _remove.Handle(
            new RemoveProjectResearchAssistantCommand(_project.Id, typed.Id, _faculty.Id, false),
            CancellationToken.None);

        var after = await _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, _admin.Id, true),
            CancellationToken.None);
        Assert.Equal(linked.Id, Assert.Single(after).Id);
    }

    [Fact]
    public async Task Admin_can_add_on_another_faculty_project()
    {
        var added = await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _admin.Id,
            IsAdmin = true,
            Name = "Lab Aide",
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        Assert.Equal("Lab Aide", added.Name);
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
                Name = "Lab Aide",
                Email = "aide@mail.aub.edu",
            },
            CancellationToken.None));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _list.Handle(
            new ListProjectResearchAssistantsQuery(_project.Id, other.Id, false),
            CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_a_duplicate_person()
    {
        await _add.Handle(new AddProjectResearchAssistantCommand
        {
            ProjectId = _project.Id,
            CurrentUserId = _faculty.Id,
            Name = "Lab Aide",
            Email = "aide@mail.aub.edu",
        }, CancellationToken.None);

        var duplicate = await Assert.ThrowsAsync<InvalidOperationException>(() => _add.Handle(
            new AddProjectResearchAssistantCommand
            {
                ProjectId = _project.Id,
                CurrentUserId = _faculty.Id,
                Name = "Someone Else",
                Email = "AIDE@mail.aub.edu",
            },
            CancellationToken.None));

        Assert.Contains("already a research assistant", duplicate.Message);
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
