using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectEditRequestedEventHandler : IEventHandler<RDProjectEditRequestedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RDProjectEditRequestedEventHandler> _logger;

        public RDProjectEditRequestedEventHandler(
            IMediator mediator,
            IUserRepository userRepository,
            ILogger<RDProjectEditRequestedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectEditRequestedEvent domainEvent)
        {
            _logger.LogInformation("Handling RDProjectEditRequestedEvent for EditRequestId: {EditRequestId}, ProjectId: {ProjectId}", 
                domainEvent.EditRequestId, domainEvent.RDProjectId);

            try
            {
                // Get all admin users to notify them about the edit request
                var adminUsers = await _userRepository.GetAdminUserIdsAsync();
                _logger.LogInformation("Found {Count} admin users for R&D project edit request notification", adminUsers.Count);

                if (!adminUsers.Any())
                {
                    _logger.LogWarning("No admin users found for R&D project edit request notification");
                    return;
                }

                // Create notifications for admins
                var successCount = 0;
                var failureCount = 0;
                
                foreach (var adminId in adminUsers)
                {
                    try
                    {
                        var command = new CreateNotificationCommand
                        {
                            UserId = adminId,
                            Title = NotificationMessages.RDProject.EditRequestedTitle(),
                            Message = NotificationMessages.RDProject.EditRequestedMessage(domainEvent.ProjectTitle, domainEvent.EditReason),
                            Type = NotificationType.RDProjectEditRequested,
                            Link = $"/rd-projects/{domainEvent.RDProjectId}/edit-request/{domainEvent.EditRequestId}",
                            Priority = "high",
                            ReferenceId = domainEvent.EditRequestId,
                            ReferenceType = "RDProjectEditRequest"
                        };
                        
                        var notificationId = await _mediator.Send(command);
                        _logger.LogInformation("Created R&D project edit request notification {NotificationId} for admin {AdminId}", notificationId, adminId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create R&D project edit request notification for admin {AdminId}", adminId);
                        failureCount++;
                    }
                }

                _logger.LogInformation("Successfully created {SuccessCount} R&D project edit request notifications, {FailureCount} failures for edit request {EditRequestId}", 
                    successCount, failureCount, domainEvent.EditRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectEditRequestedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
                throw;
            }
        }
    }
}
