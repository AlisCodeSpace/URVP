using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.Challenges
{
    /// <summary>
    /// Event handler for ChallengeEditRequestedEvent
    /// Handles notifications when a Community Partner requests an edit for their challenge
    /// </summary>
    public class ChallengeEditRequestedEventHandler : IEventHandler<ChallengeEditRequestedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ChallengeEditRequestedEventHandler> _logger;

        public ChallengeEditRequestedEventHandler(
            IMediator mediator,
            IUserRepository userRepository,
            ILogger<ChallengeEditRequestedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ChallengeEditRequestedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeEditRequestedEvent for EditRequestId: {EditRequestId}, ChallengeId: {ChallengeId}", 
                domainEvent.EditRequestId, domainEvent.ChallengeId);

            try
            {
                // Get all admin users to notify them about the edit request
                var adminUsers = await _userRepository.GetAdminUserIdsAsync();
                _logger.LogInformation("Found {Count} admin users for edit request notification", adminUsers.Count);

                if (!adminUsers.Any())
                {
                    _logger.LogWarning("No admin users found for challenge edit request notification");
                    return;
                }

                // Create notifications for admins using MediatR
                var successCount = 0;
                var failureCount = 0;
                
                foreach (var adminId in adminUsers)
                {
                    try
                    {
                        var command = new CreateNotificationCommand
                        {
                            UserId = adminId,
                            Title = NotificationMessages.Challenge.EditRequestedTitle(),
                            Message = NotificationMessages.Challenge.EditRequestedMessage(domainEvent.ChallengeTitle, domainEvent.EditReason),
                            Type = NotificationType.ChallengeEditRequested,
                            Link = $"/challenges/{domainEvent.ChallengeId}/edit-request/{domainEvent.EditRequestId}",
                            Priority = "high",
                            ReferenceId = domainEvent.EditRequestId,
                            ReferenceType = "ChallengeEditRequest"
                        };
                        
                        var notificationId = await _mediator.Send(command);
                        _logger.LogInformation("Created edit request notification {NotificationId} for admin {AdminId}", notificationId, adminId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create edit request notification for admin {AdminId}", adminId);
                        failureCount++;
                    }
                }

                _logger.LogInformation("Successfully created {SuccessCount} edit request notifications, {FailureCount} failures for edit request {EditRequestId}", 
                    successCount, failureCount, domainEvent.EditRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeEditRequestedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
                throw;
            }
        }
    }
}
