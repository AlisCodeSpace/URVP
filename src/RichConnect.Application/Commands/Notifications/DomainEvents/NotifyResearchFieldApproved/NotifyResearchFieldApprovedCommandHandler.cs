using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldApproved
{
    public class NotifyResearchFieldApprovedCommandHandler : BaseCommandHandler<NotifyResearchFieldApprovedCommand>
    {
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IMediator _mediator;

        public NotifyResearchFieldApprovedCommandHandler(
            IResearchFieldRepository researchFieldRepository,
            IMediator mediator,
            ILogger<NotifyResearchFieldApprovedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _researchFieldRepository = researchFieldRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyResearchFieldApprovedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyResearchFieldApprovedCommand for field {FieldId}", request.FieldId);

            var field = await _researchFieldRepository.GetFieldWithUserAsync(request.FieldId);

            if (field?.UserSubmitted == null)
            {
                _logger.LogWarning("Research field {FieldId} not found for notification", request.FieldId);
                return;
            }

            // Create notification for field submitter using MediatR
            var command = new CreateNotificationCommand
            {
                UserId = field.SubmittedBy,
                Title = NotificationMessages.ResearchField.ApprovedTitle(),
                Message = NotificationMessages.ResearchField.ApprovedMessage(field.Name),
                Type = NotificationType.ResearchFieldApproved,
                Link = $"/research-fields/{field.Id}",
                Priority = "medium"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for research field approval {FieldId}", request.FieldId);
        }
    }
}
