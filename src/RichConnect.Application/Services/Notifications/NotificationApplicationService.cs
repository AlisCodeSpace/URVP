using MediatR;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Commands.Notifications.MarkAsRead;
using RICHConnect.Backend.Application.Commands.Notifications.MarkAllAsRead;
using RICHConnect.Backend.Application.Commands.Notifications.DeleteNotification;
using RICHConnect.Backend.Application.Commands.Notifications.UpdateNotificationSettings;
using RICHConnect.Backend.Application.Queries.Notifications.GetNotificationById;
using RICHConnect.Backend.Application.Queries.Notifications.GetUserNotifications;
using RICHConnect.Backend.Application.Queries.Notifications.GetUnreadCount;
using RICHConnect.Backend.Application.Queries.Notifications.GetNotificationSettings;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class NotificationApplicationService : INotificationApplicationService
    {
        private readonly IMediator _mediator;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<NotificationApplicationService> _logger;
        private readonly NotificationValidationService _validationService;

        public NotificationApplicationService(
            IMediator mediator,
            INotificationRepository notificationRepository,
            IEventBus eventBus,
            ILogger<NotificationApplicationService> logger,
            NotificationValidationService validationService)
        {
            _mediator = mediator;
            _notificationRepository = notificationRepository;
            _eventBus = eventBus;
            _logger = logger;
            _validationService = validationService;
        }

        public async Task<Guid> CreateNotificationAsync(CreateNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("Creating notification for user {UserId} with type {Type}", 
                    request.UserId, request.Type);

                // Apply centralized validation
                var validationResult = await _validationService.ValidateNotificationCreationAsync(
                    request.UserId, request.Type, "push");

                if (!validationResult.IsSuccess)
                {
                    _logger.LogWarning("Notification creation blocked for user {UserId}: {ErrorMessage}", 
                        request.UserId, validationResult.ErrorMessage);
                    throw new InvalidOperationException(validationResult.ErrorMessage);
                }

                // Create command and send via MediatR
                var command = new CreateNotificationCommand
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    Link = request.Link,
                    Priority = request.Priority
                };

                var notificationId = await _mediator.Send(command);

                _logger.LogInformation("Notification created successfully with ID {NotificationId}", notificationId);
                return notificationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<Notification?> GetNotificationByIdAsync(Guid notificationId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting notification {NotificationId} for user {UserId}", 
                    notificationId, userId);

                // Validate access
                var accessValidation = await _validationService.ValidateNotificationAccessAsync(notificationId, userId);
                if (!accessValidation.IsSuccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to notification {NotificationId}: {ErrorMessage}", 
                        userId, notificationId, accessValidation.ErrorMessage);
                    return null;
                }

                var query = new GetNotificationByIdQuery
                {
                    NotificationId = notificationId,
                    UserId = userId
                };

                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId} for user {UserId}", 
                    notificationId, userId);
                throw;
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead = null)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user {UserId} - page {Page}, size {PageSize}, isRead {IsRead}", 
                    userId, page, pageSize, isRead);

                var query = new GetUserNotificationsQuery
                {
                    UserId = userId,
                    PageNumber = page,
                    PageSize = pageSize,
                    IsRead = isRead
                };

                var result = await _mediator.Send(query);
                return result.Notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting unread count for user {UserId}", userId);

                var query = new GetUnreadCountQuery
                {
                    UserId = userId
                };

                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", 
                    notificationId, userId);

                // Validate access
                var accessValidation = await _validationService.ValidateNotificationAccessAsync(notificationId, userId);
                if (!accessValidation.IsSuccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to notification {NotificationId}: {ErrorMessage}", 
                        userId, notificationId, accessValidation.ErrorMessage);
                    return false;
                }

                var command = new MarkAsReadCommand
                {
                    NotificationId = notificationId,
                    UserId = userId
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", 
                    notificationId, userId);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

                var command = new MarkAllAsReadCommand
                {
                    UserId = userId
                };

                var count = await _mediator.Send(command);
                return count > 0; // Return true if any notifications were marked as read
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", 
                    notificationId, userId);

                // Validate access
                var accessValidation = await _validationService.ValidateNotificationAccessAsync(notificationId, userId);
                if (!accessValidation.IsSuccess)
                {
                    _logger.LogWarning("User {UserId} does not have access to notification {NotificationId}: {ErrorMessage}", 
                        userId, notificationId, accessValidation.ErrorMessage);
                    return false;
                }

                var command = new DeleteNotificationCommand
                {
                    NotificationId = notificationId,
                    UserId = userId
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", 
                    notificationId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteAllNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Deleting all notifications for user {UserId}", userId);

                // Get all notifications for the user
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, 1, int.MaxValue);
                
                if (notifications.Count == 0)
                {
                    _logger.LogInformation("No notifications found for user {UserId}", userId);
                    return true;
                }

                // Delete all notifications
                foreach (var notification in notifications)
                {
                    await _notificationRepository.DeleteAsync(notification.Id, userId);
                }

                _logger.LogInformation("Successfully deleted {Count} notifications for user {UserId}", 
                    notifications.Count, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserNotificationSettings?> GetNotificationSettingsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting notification settings for user {UserId}", userId);

                var query = new GetNotificationSettingsQuery
                {
                    UserId = userId
                };

                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification settings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateNotificationSettingsAsync(UserNotificationSettings settings)
        {
            try
            {
                _logger.LogInformation("Updating notification settings for user {UserId}", settings.UserId);

                var command = new UpdateNotificationSettingsCommand
                {
                    UserId = settings.UserId,
                    EmailNotifications = settings.EmailNotifications,
                    InAppNotifications = settings.InAppNotifications
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings for user {UserId}", settings.UserId);
                throw;
            }
        }
    }
}

