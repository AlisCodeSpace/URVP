using FEA.URVP.Application.Commands.Notifications.Create;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Notifications;

/// <summary>
/// Sends <see cref="CreateNotificationCommand"/> per recipient. Not an email helper.
/// </summary>
internal static class NotificationFanOut
{
    public static async Task<int> SendAsync(
        IMediator mediator,
        ILogger logger,
        IEnumerable<Guid> userIds,
        Func<Guid, CreateNotificationCommand> factory,
        CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var userId in userIds.Distinct())
        {
            if (userId == Guid.Empty)
            {
                continue;
            }

            try
            {
                await mediator.Send(factory(userId), cancellationToken);
                created++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
            }
        }

        return created;
    }
}
