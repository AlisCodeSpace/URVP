using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Matching;

public sealed class ListAssignmentCandidatesQueryHandlerTests
{
    [Fact]
    public async Task Recommended_list_prefers_shared_research_interests_and_qualifications()
    {
        var project = new Project
        {
            Title = "Water systems",
            ResearchAreas = ["Water", "Structures"],
            MinQualifications = "Civil engineering majors in the Faculty of Engineering.",
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@mail.aub.edu",
            BriefDescription = "Research",
            VolunteersRequired = 1,
        };

        var fit = Student("Ada Lovelace", "ada@mail.aub.edu", "ada");
        var other = Student("Grace Hopper", "grace@mail.aub.edu", "grace");
        var noProfile = Student("Alan Turing", "alan@mail.aub.edu", "alan");

        var profiles = new List<StudentProfile>
        {
            Profile(fit.Id, "Faculty of Engineering", "Civil Engineering", ["Water", "Climate"]),
            Profile(other.Id, "FAS", "Biology", ["Biology"]),
        };

        var handler = Handler(project, [fit, other, noProfile], profiles, []);

        var recommended = await handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, true, 1, 10),
            CancellationToken.None);
        var all = await handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 10),
            CancellationToken.None);
        var searched = await handler.Handle(
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
    public async Task Marks_students_already_assigned_to_a_project()
    {
        var project = new Project
        {
            Title = "Water systems",
            ResearchAreas = ["Water"],
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@mail.aub.edu",
            BriefDescription = "Research",
            VolunteersRequired = 1,
        };
        var student = Student("Ada Lovelace", "ada@mail.aub.edu", "ada");
        var otherProject = new Project
        {
            Title = "Bridge sensors",
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@mail.aub.edu",
            BriefDescription = "Research",
            VolunteersRequired = 1,
        };
        var placement = new Placement
        {
            StudentUserId = student.Id,
            ProjectId = otherProject.Id,
            Project = otherProject,
            Status = PlacementStatus.Confirmed,
        };

        var handler = Handler(project, [student], [], [placement]);
        var result = await handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 10),
            CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(otherProject.Id, item.AssignedProjectId);
        Assert.Equal("Bridge sensors", item.AssignedProjectTitle);
    }

    [Fact]
    public async Task Excludes_students_already_assigned_to_this_project()
    {
        var project = new Project
        {
            Title = "Water systems",
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@mail.aub.edu",
            BriefDescription = "Research",
            VolunteersRequired = 2,
        };
        var assigned = Student("Ada Lovelace", "ada@mail.aub.edu", "ada");
        var free = Student("Grace Hopper", "grace@mail.aub.edu", "grace");
        var placement = new Placement
        {
            StudentUserId = assigned.Id,
            ProjectId = project.Id,
            Project = project,
            Status = PlacementStatus.Confirmed,
        };

        var handler = Handler(project, [assigned, free], [], [placement]);
        var result = await handler.Handle(
            new ListAssignmentCandidatesQuery(project.Id, null, false, 1, 10),
            CancellationToken.None);

        Assert.Equal(free.Id, Assert.Single(result.Items).UserId);
        Assert.Equal(1, result.TotalCount);
    }

    private static ListAssignmentCandidatesQueryHandler Handler(
        Project project,
        IReadOnlyList<User> students,
        IReadOnlyList<StudentProfile> profiles,
        IReadOnlyList<Placement> placements)
    {
        var projects = Substitute.For<IProjectRepository>();
        projects.FindByIdAsync(project.Id, Arg.Any<CancellationToken>()).Returns(project);

        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
                null,
                UserRole.Student,
                UserSortField.Name,
                SortDirection.Asc,
                false,
                false,
                Arg.Any<CancellationToken>())
            .Returns(students);

        var profileRepo = Substitute.For<IStudentProfileRepository>();
        profileRepo.ListByUserIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(profiles);

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.ListConfirmedByStudentIdsAsync(
                Arg.Any<IReadOnlyCollection<Guid>>(),
                Arg.Any<CancellationToken>())
            .Returns(placements);

        return new ListAssignmentCandidatesQueryHandler(projects, users, profileRepo, runs);
    }

    private static User Student(string name, string email, string userName) => new()
    {
        Name = name,
        Email = email,
        UserName = userName,
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
}
