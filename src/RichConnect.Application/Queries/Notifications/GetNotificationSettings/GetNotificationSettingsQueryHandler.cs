using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationSettings;

public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, UserNotificationSettings?>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetNotificationSettingsQueryHandler> _logger;

    public GetNotificationSettingsQueryHandler(
        INotificationRepository notificationRepository,
        ILogger<GetNotificationSettingsQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<UserNotificationSettings?> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification settings for user {UserId}", request.UserId);

        var settings = await _notificationRepository.GetUserSettingsAsync(request.UserId);
        
        if (settings == null)
        {
            _logger.LogInformation("No notification settings found for user {UserId}, returning default settings", request.UserId);
            
            // Return default settings if none exist
            return new UserNotificationSettings
            {
                UserId = request.UserId,
                EmailNotifications = true,
                InAppNotifications = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        _logger.LogInformation("Successfully retrieved notification settings for user {UserId}", request.UserId);
        return settings;
    }
}

