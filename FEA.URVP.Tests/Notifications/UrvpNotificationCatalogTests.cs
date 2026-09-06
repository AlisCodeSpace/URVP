using FEA.URVP.Application.Commands.Notifications.NotifyProjectOpen;
using FEA.URVP.Application.Commands.Notifications.NotifyRankingSubmitted;
using FEA.URVP.Application.Events.Projects;
using FEA.URVP.Application.Events.Rankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Events.Projects;
using FEA.URVP.Domain.Events.Rankings;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Notifications;

public sealed class UrvpNotificationCatalogTests
{
    [Fact]
    public async Task Project_opened_handler_only_sends_notify_command()
    {
        var projectId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var handler = new ProjectOpenedEventHandler(mediator);

        await handler.HandleAsync(new ProjectOpenedEvent(projectId), CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<NotifyProjectOpenCommand>(c => c.ProjectId == projectId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Project_open_does_not_notify_when_application_window_is_closed()
    {
        var semesters = Substitute.For<FEA.URVP.Application.Abstractions.Persistence.ISemesterRepository>();
        semesters.FindActiveAsync(Arg.Any<CancellationToken>()).Returns(new Semester
        {
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = DateTime.UtcNow.AddDays(-1),
        });

        var handler = new NotifyProjectOpenCommandHandler(
            Substitute.For<FEA.URVP.Application.Abstractions.Persistence.IProjectRepository>(),
            semesters,
            Substitute.For<FEA.URVP.Application.Abstractions.Persistence.IUserRepository>(),
            Substitute.For<IMediator>(),
            NullLogger<NotifyProjectOpenCommandHandler>.Instance);

        var count = await handler.Handle(new NotifyProjectOpenCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Ranking_submitted_handler_only_sends_notify_command()
    {
        var rankingId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var handler = new ProjectRankingSubmittedEventHandler(mediator);

        await handler.HandleAsync(
            new ProjectRankingSubmittedEvent(rankingId, projectId, ownerId, "Water", "Student"),
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<NotifyRankingSubmittedCommand>(c =>
                c.RankingId == rankingId && c.OwnerUserId == ownerId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Confirm_and_notify_handlers_do_not_depend_on_email()
    {
        Assert.DoesNotContain(
            typeof(FEA.URVP.Application.Abstractions.Notifications.IEmailService),
            typeof(FEA.URVP.Application.Commands.Matching.Confirm.ConfirmMatchingRunCommandHandler)
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType));

        Assert.DoesNotContain(
            typeof(FEA.URVP.Application.Abstractions.Notifications.IEmailService),
            typeof(FEA.URVP.Application.Commands.Projects.Create.CreateProjectCommandHandler)
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType));
    }

    [Fact]
    public void Unused_compatibility_types_still_exist()
    {
        _ = new Project { Title = "x" };
        Assert.True(Enum.IsDefined(FEA.URVP.Domain.Enums.NotificationType.ProjectApproved));
        Assert.True(Enum.IsDefined(FEA.URVP.Domain.Enums.NotificationType.PlacementConfirmed));
        Assert.True(Enum.IsDefined(FEA.URVP.Domain.Enums.NotificationType.FacultyRankingSubmitted));
    }
}
