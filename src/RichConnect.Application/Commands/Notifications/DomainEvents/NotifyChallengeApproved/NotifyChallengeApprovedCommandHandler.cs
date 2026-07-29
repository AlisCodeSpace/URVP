using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeApproved
{
    public class NotifyChallengeApprovedCommandHandler : BaseCommandHandler<NotifyChallengeApprovedCommand>
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IMediator _mediator;

        public NotifyChallengeApprovedCommandHandler(
            IChallengeRepository challengeRepository,
            IMediator mediator,
            ILogger<NotifyChallengeApprovedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _challengeRepository = challengeRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyChallengeApprovedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyChallengeApprovedCommand for challenge {ChallengeId}", request.ChallengeId);

            var challenge = await _challengeRepository.GetChallengeWithUserAsync(request.ChallengeId);

            if (challenge == null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found for notification", request.ChallengeId);
                return;
            }

            // Create notification for the challenge submitter
            var command = new CreateNotificationCommand
            {
                UserId = challenge.SubmittedBy,
                Title = NotificationMessages.Challenge.ApprovedTitle(),
                Message = NotificationMessages.Challenge.ApprovedMessage(challenge.Title),
                Type = NotificationType.ChallengeApproved,
                Link = $"/challenges/{challenge.Id}",
                Priority = "medium",
                ReferenceId = challenge.Id,
                ReferenceType = "Challenge"
            };
            
            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for challenge approval {ChallengeId}", request.ChallengeId);
        }
    }
}

