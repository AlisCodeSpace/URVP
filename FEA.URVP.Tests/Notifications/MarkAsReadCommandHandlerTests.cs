using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.MarkAsRead;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Notifications;

public sealed class MarkAsReadCommandHandlerTests
{
    [Fact]
    public async Task Owner_can_mark_as_read_and_event_is_published()
    {
        var ownerId = Guid.NewGuid();
        var notification = UnreadNotification(ownerId);

        var repo = Substitute.For<INotificationRepository>();
        repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);
        repo.MarkAsReadAsync(notification.Id, ownerId, Arg.Any<CancellationToken>()).Returns(true);

        var bus = Substitute.For<IEventBus>();
        var handler = CreateHandler(repo, bus);

        var result = await handler.Handle(
            new MarkAsReadCommand(notification.Id, ownerId),
            CancellationToken.None);

        Assert.True(result);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        await bus.Received(1).PublishAsync(
            Arg.Is<NotificationReadEvent>(e =>
                e.NotificationId == notification.Id && e.UserId == ownerId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Non_owner_cannot_mark_as_read()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var notification = UnreadNotification(ownerId);

        var repo = Substitute.For<INotificationRepository>();
        repo.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var bus = Substitute.For<IEventBus>();
        var handler = CreateHandler(repo, bus);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new MarkAsReadCommand(notification.Id, otherUserId), CancellationToken.None));

        Assert.False(notification.IsRead);
        await repo.DidNotReceive().MarkAsReadAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
        await bus.DidNotReceive().PublishAsync(Arg.Any<NotificationReadEvent>(), Arg.Any<CancellationToken>());
    }

    private static Notification UnreadNotification(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Type = nameof(NotificationType.ProjectApproved),
        Title = "Approved",
        Message = "Your project was approved.",
        IsRead = false,
        Priority = "high",
    };

    private static MarkAsReadCommandHandler CreateHandler(
        INotificationRepository repo,
        IEventBus bus)
    {
        var validation = new NotificationValidationService(
            new NotificationBusinessRulesService(
                repo,
                Options.Create(new NotificationSettingsOptions()),
                NullLogger<NotificationBusinessRulesService>.Instance));

        return new MarkAsReadCommandHandler(
            NullLogger<MarkAsReadCommandHandler>.Instance,
            new ImmediateUnitOfWork(),
            repo,
            validation,
            bus);
    }
}
