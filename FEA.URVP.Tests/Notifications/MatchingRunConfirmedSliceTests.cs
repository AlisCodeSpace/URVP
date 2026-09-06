using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Matching.Confirm;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Commands.Notifications.NotifyMatchingConfirmed;
using FEA.URVP.Application.Events.Matching;
using FEA.URVP.Application.Events.Notifications;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events;
using FEA.URVP.Domain.Events.Matching;
using FEA.URVP.Domain.Events.Notifications;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Notifications;

public sealed class MatchingRunConfirmedSliceTests
{
    [Fact]
    public async Task Confirm_publishes_business_event_after_commit_and_does_not_send_email()
    {
        var (run, studentId, _) = DraftRunWithOnePlacement();
        var adminId = Guid.NewGuid();
        var bus = new CapturingEventBus();
        var runs = Substitute.For<IMatchingRunRepository>();
        runs.FindByIdAsync(run.Id, Arg.Any<CancellationToken>()).Returns(run);
        runs.CountConfirmedByProjectAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(1);

        var projects = Substitute.For<IProjectRepository>();
        projects.FindByIdAsync(run.Placements[0].ProjectId, Arg.Any<CancellationToken>())
            .Returns(run.Placements[0].Project);

        var handler = new ConfirmMatchingRunCommandHandler(
            NullLogger<ConfirmMatchingRunCommandHandler>.Instance,
            new ImmediateUnitOfWork(),
            runs,
            projects,
            bus);

        await handler.Handle(new ConfirmMatchingRunCommand(run.Id, adminId), CancellationToken.None);

        var published = Assert.Single(bus.Events.OfType<MatchingRunConfirmedEvent>());
        Assert.Equal(run.Id, published.RunId);
        Assert.Equal(adminId, published.ConfirmedByUserId);
        Assert.Equal(studentId, Assert.Single(published.Placements).StudentUserId);
        Assert.DoesNotContain(bus.Events, e => e is NotificationCreatedEvent);
        Assert.DoesNotContain(
            typeof(IEmailService),
            typeof(ConfirmMatchingRunCommandHandler).GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType));
    }

    [Fact]
    public async Task Thin_handler_only_sends_notify_command()
    {
        var runId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        var handler = new MatchingRunConfirmedEventHandler(mediator);

        await handler.HandleAsync(
            new MatchingRunConfirmedEvent(runId, Guid.NewGuid(), []),
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<NotifyMatchingConfirmedCommand>(c => c.RunId == runId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task One_student_gets_one_notification_and_outbox_row_and_dedupes()
    {
        var (run, studentId, placementId) = DraftRunWithOnePlacement();
        run.Confirm(Guid.NewGuid(), DateTime.UtcNow);

        var created = new List<Notification>();
        var notifications = Substitute.For<INotificationRepository>();
        notifications.FindByReferenceAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(call => created.FirstOrDefault(n =>
                n.UserId == call.ArgAt<Guid>(0)
                && n.Type == call.ArgAt<string>(1)
                && n.ReferenceId == call.ArgAt<Guid>(2)));
        notifications.When(x => x.Create(Arg.Any<Notification>()))
            .Do(call => created.Add(call.Arg<Notification>()));
        notifications.GetSettingsAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new UserNotificationSettings
            {
                UserId = studentId,
                EmailNotifications = true,
                InAppNotifications = true,
            });
        notifications.GetUnreadCountAsync(studentId, Arg.Any<CancellationToken>()).Returns(0);

        var capturingBus = new CapturingEventBus();
        var validation = new NotificationValidationService(
            new NotificationBusinessRulesService(
                notifications,
                Options.Create(new NotificationSettingsOptions()),
                NullLogger<NotificationBusinessRulesService>.Instance));

        var createHandler = new CreateNotificationCommandHandler(
            NullLogger<CreateNotificationCommandHandler>.Instance,
            new ImmediateUnitOfWork(),
            notifications,
            validation,
            capturingBus);

        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateNotificationCommand>(), Arg.Any<CancellationToken>())
            .Returns(call => createHandler.Handle(
                call.Arg<CreateNotificationCommand>(),
                call.Arg<CancellationToken>()));

        var runs = Substitute.For<IMatchingRunRepository>();
        runs.FindByIdAsync(run.Id, Arg.Any<CancellationToken>()).Returns(run);

        var notifyHandler = new NotifyMatchingConfirmedCommandHandler(
            runs,
            mediator,
            NullLogger<NotifyMatchingConfirmedCommandHandler>.Instance);

        var first = await notifyHandler.Handle(
            new NotifyMatchingConfirmedCommand(run.Id),
            CancellationToken.None);
        var second = await notifyHandler.Handle(
            new NotifyMatchingConfirmedCommand(run.Id),
            CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(1, second);
        var notification = Assert.Single(created);
        Assert.Equal(studentId, notification.UserId);
        Assert.Equal(nameof(NotificationType.MatchingConfirmed), notification.Type);
        Assert.Equal(placementId, notification.ReferenceId);
        Assert.Equal(NotifyMatchingConfirmedCommandHandler.ReferenceType, notification.ReferenceType);
        Assert.Equal(NotificationMessages.Matching.MatchingConfirmedTitle(), notification.Title);
        Assert.Equal($"/projects/detail?id={run.Placements[0].ProjectId}", notification.Data);

        var createdEvents = capturingBus.Events.OfType<NotificationCreatedEvent>().ToList();
        Assert.Single(createdEvents);
        Assert.Equal(notification.Id, createdEvents[0].NotificationId);

        var outbox = Substitute.For<INotificationOutboxService>();
        var createdHandler = new NotificationCreatedEventHandler(
            validation,
            outbox,
            Substitute.For<IPushNotificationService>(),
            Substitute.For<INotificationCacheService>(),
            NullLogger<NotificationCreatedEventHandler>.Instance);

        await createdHandler.HandleAsync(createdEvents[0], CancellationToken.None);

        await outbox.Received(1).QueueEmailNotificationAsync(notification.Id, Arg.Any<CancellationToken>());
    }

    private static (MatchingRun Run, Guid StudentId, Guid PlacementId) DraftRunWithOnePlacement()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var placementId = Guid.NewGuid();
        var semester = new Semester { Name = "Fall 2026" };
        var student = new User
        {
            Id = studentId,
            Email = "student@mail.aub.edu",
            Name = "Student",
            UserName = "student",
            Affiliation = "FEA",
            Role = UserRole.Student,
        };
        var project = new Project
        {
            Id = projectId,
            Title = "Water systems",
            FacultyNameSnapshot = "Faculty",
            AffiliationSnapshot = "FEA",
            EmailSnapshot = "faculty@mail.aub.edu",
            BriefDescription = "Research",
        };
        var placement = new Placement
        {
            Id = placementId,
            ProjectId = projectId,
            Project = project,
            StudentUserId = studentId,
            StudentUser = student,
            StudentRank = 1,
            FacultyRank = 1,
            Status = PlacementStatus.Proposed,
        };
        var run = new MatchingRun
        {
            SemesterId = semester.Id,
            Semester = semester,
            AlgorithmVersion = "da-student-proposing/v1",
            Seed = 1,
            Placements = [placement],
        };
        placement.MatchingRunId = run.Id;
        return (run, studentId, placementId);
    }

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
