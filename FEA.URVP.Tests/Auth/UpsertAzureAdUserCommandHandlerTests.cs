using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Auth.AzureAd;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Auth;

public sealed class UpsertAzureAdUserCommandHandlerTests
{
    [Fact]
    public async Task New_student_mailbox_is_not_stored_as_faculty_when_groups_are_missing()
    {
        var added = await CreateAsync("aa624@mail.aub.edu", directoryGroupRole: null);

        Assert.Equal(UserRole.Student, added.Role);
    }

    [Fact]
    public async Task Directory_student_role_is_used_even_for_an_aub_edu_lb_mailbox()
    {
        var added = await CreateAsync("ta@aub.edu.lb", directoryGroupRole: UserRole.Student);

        Assert.Equal(UserRole.Student, added.Role);
    }

    [Fact]
    public async Task Existing_student_misclassified_as_faculty_is_corrected_from_mailbox_domain()
    {
        var existing = new User
        {
            Email = "aa624@mail.aub.edu",
            Name = "Student",
            UserName = "aa624",
            Affiliation = "AUB",
            Role = UserRole.Faculty,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var users = Substitute.For<IUserRepository>();
        users.FindByEmailAsync("aa624@mail.aub.edu", Arg.Any<CancellationToken>()).Returns(existing);

        var handler = Handler(users);
        await handler.Handle(
            new UpsertAzureAdUserCommand(
                "aa624@mail.aub.edu",
                "Student",
                "aa624",
                "AUB"),
            CancellationToken.None);

        Assert.Equal(UserRole.Student, existing.Role);
    }

    [Fact]
    public async Task Stored_admin_is_not_overwritten_by_mailbox_or_groups()
    {
        var existing = new User
        {
            Email = "aa624@mail.aub.edu",
            Name = "Admin",
            UserName = "aa624",
            Affiliation = "AUB",
            Role = UserRole.Admin,
            RegisteredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var users = Substitute.For<IUserRepository>();
        users.FindByEmailAsync("aa624@mail.aub.edu", Arg.Any<CancellationToken>()).Returns(existing);

        var handler = Handler(users);
        await handler.Handle(
            new UpsertAzureAdUserCommand(
                "aa624@mail.aub.edu",
                "Admin",
                "aa624",
                "AUB",
                directoryGroupRole: UserRole.Student),
            CancellationToken.None);

        Assert.Equal(UserRole.Admin, existing.Role);
    }

    [Fact]
    public async Task Unknown_mailbox_defaults_a_new_user_to_student_not_faculty()
    {
        var added = await CreateAsync("visitor@example.com", directoryGroupRole: null);

        Assert.Equal(UserRole.Student, added.Role);
    }

    private static async Task<User> CreateAsync(string email, UserRole? directoryGroupRole)
    {
        User? added = null;
        var users = Substitute.For<IUserRepository>();
        users.FindByEmailAsync(email.ToLowerInvariant(), Arg.Any<CancellationToken>()).Returns((User?)null);
        users.When(x => x.Add(Arg.Any<User>())).Do(call => added = call.Arg<User>());

        var handler = Handler(users);
        await handler.Handle(
            new UpsertAzureAdUserCommand(email, "Name", "user", "AUB", directoryGroupRole: directoryGroupRole),
            CancellationToken.None);

        Assert.NotNull(added);
        return added!;
    }

    private static UpsertAzureAdUserCommandHandler Handler(IUserRepository users)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new UpsertAzureAdUserCommandHandler(
            NullLogger<UpsertAzureAdUserCommandHandler>.Instance,
            Substitute.For<IUnitOfWork>(),
            users,
            configuration);
    }
}
