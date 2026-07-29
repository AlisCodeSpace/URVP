using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldSubmitted
{
    public class NotifyResearchFieldSubmittedCommandHandler : BaseCommandHandler<NotifyResearchFieldSubmittedCommand>
    {
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyResearchFieldSubmittedCommandHandler(
            IResearchFieldRepository researchFieldRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyResearchFieldSubmittedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _researchFieldRepository = researchFieldRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyResearchFieldSubmittedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyResearchFieldSubmittedCommand for field {FieldId}", request.FieldId);

            var field = await _researchFieldRepository.GetFieldWithUserAsync(request.FieldId);

            if (field?.UserSubmitted == null)
            {
                _logger.LogWarning("Research field {FieldId} not found for notification", request.FieldId);
                return;
            }

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for research field submission notification");
                return;
            }

            // Create notifications for admins using MediatR
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.ResearchField.SubmittedTitle(),
                    Message = NotificationMessages.ResearchField.SubmittedMessage(field.Name),
                    Type = NotificationType.ResearchFieldSubmitted,
                    Link = $"/research-fields/{field.Id}",
                    Priority = "medium",
                    ReferenceId = field.Id,
                    ReferenceType = "ResearchField"
                };

                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for research field submission {FieldId}", 
                adminUsers.Count, request.FieldId);
        }
    }
}
