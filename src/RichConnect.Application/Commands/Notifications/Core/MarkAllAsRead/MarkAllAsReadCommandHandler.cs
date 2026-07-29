using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<MarkAllAsReadCommandHandler> _logger;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ILogger<MarkAllAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<int> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking all notifications as read for user {UserId}", request.UserId);

        // Get current unread count for logging
        var unreadCount = await _notificationRepository.GetUnreadCountAsync(request.UserId);
        
        // Mark all notifications as read
        await _notificationRepository.MarkAllAsReadAsync(request.UserId);
        
        _logger.LogInformation("Successfully marked {Count} notifications as read for user {UserId}", 
            unreadCount, request.UserId);

        return unreadCount;
    }
}

