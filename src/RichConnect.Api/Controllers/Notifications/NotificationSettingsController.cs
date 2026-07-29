using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.DTOs.Notifications;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Application.Queries.Notifications.GetNotificationSettings;
using RICHConnect.Backend.Application.Commands.Notifications.UpdateNotificationSettings;

namespace RICHConnect.Backend.Api.Controllers.Notifications
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationSettingsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationApplicationService _notificationApplicationService;

        public NotificationSettingsController(IMediator mediator, INotificationApplicationService notificationApplicationService)
        {
            _mediator = mediator;
            _notificationApplicationService = notificationApplicationService;
        }

        /// <summary>
        /// Get user's notification settings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var userId = GetCurrentUserId();
            
            var query = new GetNotificationSettingsQuery
            {
                UserId = userId
            };
            
            var settings = await _mediator.Send(query);

            if (settings == null)
            {
                // Create default settings using the application service
                var defaultSettings = new UserNotificationSettings
                {
                    UserId = userId,
                    EmailNotifications = true,
                    InAppNotifications = true
                };
                
                await _notificationApplicationService.UpdateNotificationSettingsAsync(defaultSettings);
                settings = defaultSettings;
            }

            var dto = new UserNotificationSettingsDto
            {
                EmailNotifications = settings.EmailNotifications,
                InAppNotifications = settings.InAppNotifications
            };

            return SuccessResponse(dto);
        }

        /// <summary>
        /// Update user's notification settings
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UserNotificationSettingsDto dto)
        {
            var userId = GetCurrentUserId();
            
            var command = new UpdateNotificationSettingsCommand
            {
                UserId = userId,
                EmailNotifications = dto.EmailNotifications,
                InAppNotifications = dto.InAppNotifications
            };

            var success = await _mediator.Send(command);

            if (!success)
                return ErrorResponse<object>("Failed to update notification settings.");

            return SuccessResponse(dto, "Notification settings updated successfully.");
        }
    }
}
