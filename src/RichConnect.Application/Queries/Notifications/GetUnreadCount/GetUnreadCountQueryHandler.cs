using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;

    public GetUnreadCountQueryHandler(
        INotificationRepository notificationRepository,
        ILogger<GetUnreadCountQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting unread count for user {UserId}", request.UserId);

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(request.UserId);
        
        _logger.LogInformation("User {UserId} has {Count} unread notifications", 
            request.UserId, unreadCount);

        return unreadCount;
    }
}

