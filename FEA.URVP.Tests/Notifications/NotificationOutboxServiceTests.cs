using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Notifications;

public sealed class NotificationOutboxServiceTests
{
    [Fact]
    public async Task ProcessOutbox_success_marks_completed()
    {
        var notification = NewNotification();
        var item = NewOutbox(notification.Id);
        var (service, outbox, email) = CreateService(notification, item, sendSucceeds: true);

        await service.ProcessOutboxAsync(CancellationToken.None);

        await email.Received(1).SendEmailAsync(
            "student@mail.aub.edu",
            "Student",
            notification.Title,
            notification.Message,
            null,
            null,
            Arg.Any<CancellationToken>());
        await outbox.Received().UpdateStatusAsync(
            item.Id,
            NotificationOutboxStatus.Completed,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await outbox.DidNotReceive().IncrementRetryAsync(
            Arg.Any<Guid>(),
            Arg.Any<string?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessOutbox_send_false_schedules_retry()
    {
        var notification = NewNotification();
        var item = NewOutbox(notification.Id);
        var (service, outbox, _) = CreateService(notification, item, sendSucceeds: false);

        await service.ProcessOutboxAsync(CancellationToken.None);

        await outbox.Received().IncrementRetryAsync(
            item.Id,
            Arg.Any<string?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>());
        await outbox.Received().UpdateStatusAsync(
            item.Id,
            NotificationOutboxStatus.Pending,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await outbox.DidNotReceive().UpdateStatusAsync(
            item.Id,
            NotificationOutboxStatus.Completed,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueEmailNotification_inserts_pending_email_row()
    {
        NotificationOutbox? created = null;
        var outbox = Substitute.For<INotificationOutboxRepository>();
        outbox.GetByNotificationIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((NotificationOutbox?)null);
        outbox.When(x => x.Create(Arg.Any<NotificationOutbox>()))
            .Do(call => created = call.Arg<NotificationOutbox>());

        var service = new NotificationOutboxService(
            outbox,
            Substitute.For<INotificationRepository>(),
            Substitute.For<IEmailService>(),
            Substitute.For<IUserEmailService>(),
            new ImmediateUnitOfWork(),
            Options.Create(new EmailOptions()),
            NullLogger<NotificationOutboxService>.Instance);

        var notificationId = Guid.NewGuid();
        await service.QueueEmailNotificationAsync(notificationId, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(notificationId, created.NotificationId);
        Assert.Equal(NotificationOutboxEventTypes.EmailNotification, created.EventType);
        Assert.Equal(nameof(NotificationOutboxStatus.Pending), created.Status);
    }

    private static Notification NewNotification() => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Type = nameof(NotificationType.ProjectApproved),
        Title = NotificationMessages.Projects.ProjectApprovedTitle(),
        Message = NotificationMessages.Projects.ProjectApprovedMessage("Water systems"),
        Priority = "high",
    };

    private static NotificationOutbox NewOutbox(Guid notificationId) => new()
    {
        Id = Guid.NewGuid(),
        NotificationId = notificationId,
        EventType = NotificationOutboxEventTypes.EmailNotification,
        Status = nameof(NotificationOutboxStatus.Pending),
    };

    private static (
        NotificationOutboxService Service,
        INotificationOutboxRepository Outbox,
        IEmailService Email) CreateService(
        Notification notification,
        NotificationOutbox item,
        bool sendSucceeds)
    {
        var outbox = Substitute.For<INotificationOutboxRepository>();
        outbox.GetPendingItemsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new[] { item });

        var notifications = Substitute.For<INotificationRepository>();
        notifications.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var email = Substitute.For<IEmailService>();
        email.SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(sendSucceeds);

        var users = Substitute.For<IUserEmailService>();
        users.GetUserEmailAsync(notification.UserId, Arg.Any<CancellationToken>())
            .Returns("student@mail.aub.edu");
        users.GetUserNameAsync(notification.UserId, Arg.Any<CancellationToken>())
            .Returns("Student");

        var service = new NotificationOutboxService(
            outbox,
            notifications,
            email,
            users,
            new ImmediateUnitOfWork(),
            Options.Create(new EmailOptions()),
            NullLogger<NotificationOutboxService>.Instance);

        return (service, outbox, email);
    }
}
