using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetUserNotifications;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, GetUserNotificationsResult>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetUserNotificationsQueryHandler> _logger;

    public GetUserNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ILogger<GetUserNotificationsQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<GetUserNotificationsResult> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notifications for user {UserId}, page {PageNumber}, size {PageSize}", 
            request.UserId, request.PageNumber, request.PageSize);

        // Get notifications with pagination
        var notifications = await _notificationRepository.GetUserNotificationsAsync(
            request.UserId, 
            request.PageNumber, 
            request.PageSize, 
            request.IsRead);

        // Get total count for pagination info
        var totalCount = await GetTotalCountAsync(request.UserId, request.IsRead);

        var result = new GetUserNotificationsResult
        {
            Notifications = notifications,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        _logger.LogInformation("Retrieved {Count} notifications for user {UserId}", 
            notifications.Count, request.UserId);

        return result;
    }

    private async Task<int> GetTotalCountAsync(Guid userId, bool? isRead)
    {
        return await _notificationRepository.GetTotalCountAsync(userId, isRead);
    }
}

