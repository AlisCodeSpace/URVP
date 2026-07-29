using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeSubmitted
{
    public class NotifyChallengeSubmittedCommandHandler : BaseCommandHandler<NotifyChallengeSubmittedCommand>
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyChallengeSubmittedCommandHandler(
            IChallengeRepository challengeRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyChallengeSubmittedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _challengeRepository = challengeRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyChallengeSubmittedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyChallengeSubmittedCommand for challenge {ChallengeId}", request.ChallengeId);

            var challenge = await _challengeRepository.GetChallengeWithUserAsync(request.ChallengeId);

            if (challenge == null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found for notification", request.ChallengeId);
                return;
            }

            _logger.LogInformation("Challenge found: {ChallengeTitle} submitted by {SubmittedBy}", 
                challenge.Title, challenge.SubmittedBy);

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();
            _logger.LogInformation("Found {Count} admin users for notification", adminUsers.Count);

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for challenge submission notification");
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
                        Title = NotificationMessages.Challenge.SubmittedTitle(),
                        Message = NotificationMessages.Challenge.SubmittedMessage(challenge.Title),
                        Type = NotificationType.ChallengeSubmitted,
                        Link = $"/challenges/{challenge.Id}",
                        Priority = "high",
                        ReferenceId = challenge.Id,
                        ReferenceType = "Challenge"
                    };
                    
                    var notificationId = await _mediator.Send(command, cancellationToken);
                    _logger.LogInformation("Created notification {NotificationId} for admin {AdminId}", notificationId, adminId);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create notification for admin {AdminId}", adminId);
                    failureCount++;
                }
            }

            _logger.LogInformation("Successfully created {SuccessCount} notifications, {FailureCount} failures for challenge submission {ChallengeId}", 
                successCount, failureCount, request.ChallengeId);
        }
    }
}
