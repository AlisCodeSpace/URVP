using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Notifications;

public sealed class CreateNotificationCommandHandlerTests
{
    [Fact]
    public async Task Creates_a_row_and_publishes_after_commit()
    {
        var userId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();
        Notification? created = null;

        var repo = Substitute.For<INotificationRepository>();
        repo.FindByReferenceAsync(userId, nameof(NotificationType.ProjectApproved), referenceId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);
        repo.When(x => x.Create(Arg.Any<Notification>()))
            .Do(call => created = call.Arg<Notification>());

        var bus = Substitute.For<IEventBus>();
        var handler = CreateHandler(repo, bus);

        var id = await handler.Handle(
            new CreateNotificationCommand(
                userId,
                "Project approved",
                "Your project was approved.",
                NotificationType.ProjectApproved,
                "/projects/" + referenceId,
                null,
                referenceId,
                "Project"),
            CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(id, created.Id);
        Assert.Equal(userId, created.UserId);
        Assert.Equal(nameof(NotificationType.ProjectApproved), created.Type);
        Assert.Equal("/projects/" + referenceId, created.Data);
        Assert.Equal("high", created.Priority);
        Assert.False(created.IsRead);
        repo.Received(1).Create(created);
        await bus.Received(1).PublishAsync(
            Arg.Is<NotificationCreatedEvent>(e =>
                e.NotificationId == created.Id
                && e.UserId == userId
                && e.Type == nameof(NotificationType.ProjectApproved)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Duplicate_user_type_reference_returns_existing_id_without_publishing()
    {
        var userId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();
        var existing = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = nameof(NotificationType.ProjectApproved),
            Title = "Existing",
            Message = "Already created",
            ReferenceId = referenceId,
            Priority = "high",
        };

        var repo = Substitute.For<INotificationRepository>();
        repo.FindByReferenceAsync(userId, nameof(NotificationType.ProjectApproved), referenceId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var bus = Substitute.For<IEventBus>();
        var handler = CreateHandler(repo, bus);

        var id = await handler.Handle(
            new CreateNotificationCommand(
                userId,
                "Project approved",
                "Your project was approved.",
                NotificationType.ProjectApproved,
                null,
                null,
                referenceId,
                "Project"),
            CancellationToken.None);

        Assert.Equal(existing.Id, id);
        repo.DidNotReceive().Create(Arg.Any<Notification>());
        await bus.DidNotReceive().PublishAsync(Arg.Any<NotificationCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Create_handler_does_not_depend_on_email_or_outbox()
    {
        var parameterTypes = typeof(CreateNotificationCommandHandler)
            .GetConstructors()
            .SelectMany(ctor => ctor.GetParameters())
            .Select(p => p.ParameterType)
            .ToList();

        Assert.DoesNotContain(typeof(IEmailService), parameterTypes);
        Assert.DoesNotContain(typeof(INotificationOutboxService), parameterTypes);
    }

    private static CreateNotificationCommandHandler CreateHandler(
        INotificationRepository repo,
        IEventBus bus)
    {
        var validation = new NotificationValidationService(
            new NotificationBusinessRulesService(
                repo,
                Options.Create(new NotificationSettingsOptions()),
                NullLogger<NotificationBusinessRulesService>.Instance));

        return new CreateNotificationCommandHandler(
            NullLogger<CreateNotificationCommandHandler>.Instance,
            new ImmediateUnitOfWork(),
            repo,
            validation,
            bus);
    }
}
