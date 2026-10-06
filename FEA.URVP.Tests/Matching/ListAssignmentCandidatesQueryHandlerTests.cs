using FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using FEA.URVP.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FEA.URVP.Tests.Matching;

public sealed class ListAssignmentCandidatesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ListAssignmentCandidatesQueryHandler _handler;
    private readonly Semester _semester = new() { Name = "Fall 2026", IsActive = true };

    public ListAssignmentCandidatesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _db.Semesters.Add(_semester);
        _db.SaveChanges();

        var projects = Substitute.For<FEA.URVP.Application.Abstractions.Persistence.IProjectRepository>();
        projects.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => _db.Projects.AsNoTracking()
                .FirstOrDefaultAsync(project => project.Id == call.Arg<Guid>()));

        _handler = new ListAssignmentCandidatesQueryHandler(
            projects,
            new AssignmentCandidateReadRepository(_db));
    }

    [Fact]
    public async Task Recommended_list_prefers_shared_research_interests_and_qualifications()
    {
        var project = Project("Water systems", ["Water", "Structures"], "Civil engineering majors in the Faculty of Engineering.");
        var fit = Student("Ada Lovelace", "ada@mail.aub.edu");
        var other = Student("Grace Hopper", "grace@mail.aub.edu");
        var noProfile = Student("Alan Turing", "alan@mail.aub.edu");
        _db.Projects.Add(project);
        _db.Users.AddRange(fit, other, noProfile);
        _db.StudentProfiles.AddRange(
            Profile(fit.Id, "Faculty of Engineering", "Civil Engineering", ["Water", "Climate"]),
            Profile(other.Id, "FAS", "Biology", ["Biology"]));
        await _db.SaveChangesAsync();

        var recommended = await _handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, true, 1, 10),
            CancellationToken.None);
        var all = await _handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 10),
            CancellationToken.None);
        var searched = await _handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, "biology", false, 1, 10),
            CancellationToken.None);

        Assert.Equal(1, recommended.TotalCount);
        Assert.Equal(fit.Id, recommended.Items[0].UserId);
        Assert.Equal(["Water"], recommended.Items[0].MatchedResearchTopics);
        Assert.True(recommended.Items[0].QualificationsMentionMajor);
        Assert.True(recommended.Items[0].QualificationsMentionFaculty);

        Assert.Equal(3, all.TotalCount);
        Assert.Equal(
            ["Ada Lovelace", "Alan Turing", "Grace Hopper"],
            all.Items.Select(item => item.Name).ToArray());
        Assert.False(all.Items.Single(item => item.UserId == noProfile.Id).HasProfile);

        Assert.Equal(other.Id, Assert.Single(searched.Items).UserId);
    }

    [Fact]
    public async Task Excludes_students_already_assigned_to_another_project()
    {
        var project = Project("Water systems", ["Water"], null);
        var otherProject = Project("Bridge sensors", ["Structures"], null);
        var assigned = Student("Ada Lovelace", "ada@mail.aub.edu");
        var free = Student("Grace Hopper", "grace@mail.aub.edu");
        _db.Projects.AddRange(project, otherProject);
        _db.Users.AddRange(assigned, free);
        await _db.SaveChangesAsync();
        Place(assigned.Id, otherProject);

        var result = await _handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 10),
            CancellationToken.None);

        Assert.Equal(free.Id, Assert.Single(result.Items).UserId);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Excludes_students_already_assigned_to_this_project_and_pages()
    {
        var project = Project("Water systems", [], null);
        project.VolunteersRequired = 8;
        var assigned = Student("Ada Lovelace", "ada@mail.aub.edu");
        var others = Enumerable.Range(1, 7)
            .Select(index => Student($"Student {index:00}", $"s{index}@mail.aub.edu"))
            .ToArray();
        _db.Projects.Add(project);
        _db.Users.Add(assigned);
        _db.Users.AddRange(others);
        await _db.SaveChangesAsync();
        Place(assigned.Id, project);

        var page = await _handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 6),
            CancellationToken.None);

        Assert.Equal(7, page.TotalCount);
        Assert.Equal(6, page.Items.Count);
        Assert.DoesNotContain(page.Items, item => item.UserId == assigned.Id);
    }

    private Project Project(string title, List<string> areas, string? qualifications) => new()
    {
        Title = title,
        ResearchAreas = areas,
        ActivityTypes = ["Lab"],
        MinQualifications = qualifications,
        FacultyNameSnapshot = "Faculty",
        AffiliationSnapshot = "FEA",
        EmailSnapshot = "faculty@mail.aub.edu",
        BriefDescription = "Research",
        VolunteersRequired = 2,
        SemesterId = _semester.Id,
        CreatedByUserId = Guid.NewGuid(),
    };

    private void Place(Guid studentUserId, Project project)
    {
        var run = new MatchingRun
        {
            SemesterId = _semester.Id,
            Status = MatchingRunStatus.Confirmed,
            AlgorithmVersion = MatchingRun.ManualAlgorithmVersion,
            CreatedByUserId = Guid.NewGuid(),
        };
        _db.MatchingRuns.Add(run);
        _db.Placements.Add(new Placement
        {
            MatchingRunId = run.Id,
            StudentUserId = studentUserId,
            ProjectId = project.Id,
            Status = PlacementStatus.Confirmed,
            StudentRank = 1,
            FacultyRank = 1,
        });
        _db.SaveChanges();
    }

    private static User Student(string name, string email) => new()
    {
        Name = name,
        Email = email,
        UserName = email.Split('@')[0],
        Affiliation = "AUB",
        Role = UserRole.Student,
    };

    private static StudentProfile Profile(
        Guid userId,
        string faculty,
        string major,
        List<string> topics) => new()
    {
        UserId = userId,
        Gender = "Female",
        MobileNumber = "03000000",
        Degree = "BE",
        Faculty = faculty,
        Major = major,
        ExpectedGraduationYear = 2027,
        ResearchTopics = topics,
        CompletedCredits = true,
        CumulativeAverage = 85,
    };

    public void Dispose() => _db.Dispose();
}
