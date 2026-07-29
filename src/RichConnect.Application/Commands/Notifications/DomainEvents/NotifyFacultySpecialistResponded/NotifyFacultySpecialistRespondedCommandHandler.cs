using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistResponded
{
    public class NotifyFacultySpecialistRespondedCommandHandler : BaseCommandHandler<NotifyFacultySpecialistRespondedCommand>
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyFacultySpecialistRespondedCommandHandler(
            IChallengeRepository challengeRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyFacultySpecialistRespondedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _challengeRepository = challengeRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyFacultySpecialistRespondedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyFacultySpecialistRespondedCommand for invite {InviteId}, challenge {ChallengeId}", 
                request.InviteId, request.ChallengeId);

            var challenge = await _challengeRepository.GetChallengeWithUserAsync(request.ChallengeId);

            if (challenge == null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found for notification", request.ChallengeId);
                return;
            }

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for faculty specialist response notification");
                return;
            }

            // Create notifications for admins using MediatR
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.FacultySpecialist.RespondedTitle(request.ResponseText),
                    Message = NotificationMessages.FacultySpecialist.RespondedMessage(request.FacultySpecialistName, challenge.Title, request.ResponseText),
                    Type = NotificationType.FacultySpecialistResponded,
                    Link = $"/challenges/{challenge.Id}",
                    Priority = "medium",
                    ReferenceId = request.InviteId,
                    ReferenceType = "Invite"
                };
                
                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for faculty specialist response to invite {InviteId}", 
                adminUsers.Count, request.InviteId);
        }
    }
}

