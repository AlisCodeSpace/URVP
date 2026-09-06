using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Events.Notifications;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FEA.URVP.Tests.Notifications;

public sealed class NotificationCreatedEventHandlerTests
{
    [Fact]
    public async Task Email_prefs_on_queues_outbox_and_does_not_throw()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var (handler, outbox, email, notifications) = CreateHandler(emailEnabled: true);

        notifications.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>()).Returns(1);

        await handler.HandleAsync(CreatedEvent(notificationId, userId), CancellationToken.None);

        await outbox.Received(1).QueueEmailNotificationAsync(notificationId, Arg.Any<CancellationToken>());
        await email.DidNotReceive().SendEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_prefs_off_skips_outbox_and_keeps_going()
    {
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var (handler, outbox, _, _) = CreateHandler(emailEnabled: false);

        await handler.HandleAsync(CreatedEvent(notificationId, userId), CancellationToken.None);

        await outbox.DidNotReceive().QueueEmailNotificationAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Outbox_and_cache_failures_do_not_throw()
    {
        var userId = Guid.NewGuid();
        var (handler, outbox, _, notifications) = CreateHandler(emailEnabled: true);
        notifications.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>()).Returns(1);
        outbox.QueueEmailNotificationAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("queue down"));

        var exception = await Record.ExceptionAsync(() =>
            handler.HandleAsync(CreatedEvent(Guid.NewGuid(), userId), CancellationToken.None));

        Assert.Null(exception);
    }

    private static NotificationCreatedEvent CreatedEvent(Guid notificationId, Guid userId) =>
        new(
            notificationId,
            userId,
            NotificationMessages.Projects.ProjectApprovedTitle(),
            NotificationMessages.Projects.ProjectApprovedMessage("Water systems"),
            nameof(NotificationType.ProjectApproved),
            "/projects/" + notificationId,
            "high");

    private static (
        NotificationCreatedEventHandler Handler,
        INotificationOutboxService Outbox,
        IEmailService Email,
        INotificationRepository Notifications) CreateHandler(bool emailEnabled)
    {
        var notifications = Substitute.For<INotificationRepository>();
        notifications.GetSettingsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new UserNotificationSettings
            {
                EmailNotifications = emailEnabled,
                InAppNotifications = true,
            });

        var validation = new NotificationValidationService(
            new NotificationBusinessRulesService(
                notifications,
                Options.Create(new NotificationSettingsOptions()),
                NullLogger<NotificationBusinessRulesService>.Instance));

        var outbox = Substitute.For<INotificationOutboxService>();
        var email = Substitute.For<IEmailService>();
        var push = Substitute.For<IPushNotificationService>();
        var cache = Substitute.For<INotificationCacheService>();

        var handler = new NotificationCreatedEventHandler(
            validation,
            outbox,
            push,
            cache,
            NullLogger<NotificationCreatedEventHandler>.Instance);

        return (handler, outbox, email, notifications);
    }
}
