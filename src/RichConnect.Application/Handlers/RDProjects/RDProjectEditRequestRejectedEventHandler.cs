using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectEditRequestRejectedEventHandler : IEventHandler<RDProjectEditRequestRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectEditRequestRejectedEventHandler> _logger;

        public RDProjectEditRequestRejectedEventHandler(
            IMediator mediator,
            ILogger<RDProjectEditRequestRejectedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectEditRequestRejectedEvent domainEvent)
        {
            _logger.LogInformation("Handling RDProjectEditRequestRejectedEvent for EditRequestId: {EditRequestId}, ProjectId: {ProjectId}", 
                domainEvent.EditRequestId, domainEvent.RDProjectId);

            try
            {
                // Send notification to the Community Partner who requested the edit
                var command = new CreateNotificationCommand
                {
                    UserId = domainEvent.RequestedBy,
                    Title = NotificationMessages.RDProject.EditRequestRejectedTitle(),
                    Message = NotificationMessages.RDProject.EditRequestRejectedMessage(domainEvent.AdminResponse),
                    Type = NotificationType.RDProjectEditRequestRejected,
                    Link = $"/rd-projects/{domainEvent.RDProjectId}",
                    Priority = "medium",
                    ReferenceId = domainEvent.EditRequestId,
                    ReferenceType = "RDProjectEditRequest"
                };
                
                var notificationId = await _mediator.Send(command);
                _logger.LogInformation("Created R&D project edit request rejected notification {NotificationId} for user {UserId}", 
                    notificationId, domainEvent.RequestedBy);

                _logger.LogInformation("Successfully processed RDProjectEditRequestRejectedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectEditRequestRejectedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
                throw;
            }
        }
    }
}
