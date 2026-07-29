using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetNotificationById;

public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Notification?>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly NotificationBusinessRulesService _businessRulesService;
    private readonly ILogger<GetNotificationByIdQueryHandler> _logger;

    public GetNotificationByIdQueryHandler(
        INotificationRepository notificationRepository,
        NotificationBusinessRulesService businessRulesService,
        ILogger<GetNotificationByIdQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _businessRulesService = businessRulesService;
        _logger = logger;
    }

    public async Task<Notification?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting notification {NotificationId} for user {UserId}", 
            request.NotificationId, request.UserId);

        // Get notification by ID
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
        
        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found", request.NotificationId);
            return null;
        }

        // Verify notification belongs to user
        var hasAccess = await _businessRulesService.ValidateNotificationAccess(
            request.NotificationId, request.UserId);
        
        if (!hasAccess)
        {
            _logger.LogWarning("User {UserId} does not have access to notification {NotificationId}", 
                request.UserId, request.NotificationId);
            throw new UnauthorizedAccessException("You do not have access to this notification");
        }

        _logger.LogInformation("Successfully retrieved notification {NotificationId} for user {UserId}", 
            request.NotificationId, request.UserId);

        return notification;
    }
}

