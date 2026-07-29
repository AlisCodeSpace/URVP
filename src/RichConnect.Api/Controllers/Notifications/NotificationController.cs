using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Queries.Notifications.GetUserNotifications;
using RICHConnect.Backend.Application.Queries.Notifications.GetUnreadCount;
using RICHConnect.Backend.Application.Commands.Notifications.MarkAsRead;
using RICHConnect.Backend.Application.Commands.Notifications.MarkAllAsRead;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Api.Controllers.Notifications
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationApplicationService _notificationApplicationService;

        public NotificationController(IMediator mediator, INotificationApplicationService notificationApplicationService)
        {
            _mediator = mediator;
            _notificationApplicationService = notificationApplicationService;
        }

        /// <summary>
        /// Get user's notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            
            var query = new GetUserNotificationsQuery
            {
                UserId = userId,
                PageNumber = page,
                PageSize = pageSize,
                IsRead = unreadOnly ? false : null
            };
            
            var result = await _mediator.Send(query);
            return SuccessResponse(result);
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            
            var query = new GetUnreadCountQuery
            {
                UserId = userId
            };
            
            var count = await _mediator.Send(query);
            return SuccessResponse(new { count });
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost("{id:guid}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            
            try
            {
                var command = new MarkAsReadCommand
                {
                    NotificationId = id,
                    UserId = userId
                };
                
                var success = await _mediator.Send(command);
                
                if (!success)
                    return ResourceNotFound("Notification", id);
                
                return SuccessResponse<object?>(null, "Notification marked as read.");
            }
            catch (ArgumentException)
            {
                return ResourceNotFound("Notification", id);
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            
            var command = new MarkAllAsReadCommand
            {
                UserId = userId
            };
            
            var count = await _mediator.Send(command);
            return SuccessResponse<object>(new { count }, $"{count} notifications marked as read.");
        }

        /// <summary>
        /// Delete all notifications
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = GetCurrentUserId();
            
            // For now, we'll use the application service since there's no DeleteAllNotifications command
            // This could be implemented as a command in the future if needed
            await _notificationApplicationService.DeleteAllNotificationsAsync(userId);
            return SuccessResponse<object?>(null, "All notifications deleted successfully.");
        }

        /// <summary>
        /// Test notification system by creating a test notification
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestNotification()
        {
            var userId = GetCurrentUserId();
            
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Title = "Test Notification",
                Message = "This is a test notification to verify the notification system is working.",
                Type = NotificationType.ChallengeSubmitted,
                Link = "/dashboard/notifications",
                Priority = "high"
            };
            
            var notificationId = await _notificationApplicationService.CreateNotificationAsync(request);
            return SuccessResponse(new { NotificationId = notificationId }, "Test notification created successfully.");
        }
    }
}
