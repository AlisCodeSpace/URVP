using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldRejected
{
    public class NotifyResearchFieldRejectedCommandHandler : BaseCommandHandler<NotifyResearchFieldRejectedCommand>
    {
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IMediator _mediator;

        public NotifyResearchFieldRejectedCommandHandler(
            IResearchFieldRepository researchFieldRepository,
            IMediator mediator,
            ILogger<NotifyResearchFieldRejectedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _researchFieldRepository = researchFieldRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyResearchFieldRejectedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyResearchFieldRejectedCommand for field {FieldId}", request.FieldId);

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
                Title = NotificationMessages.ResearchField.RejectedTitle(),
                Message = NotificationMessages.ResearchField.RejectedMessage(field.Name, request.RejectionReason),
                Type = NotificationType.ResearchFieldRejected,
                Link = $"/research-fields/{field.Id}",
                Priority = "high"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for research field rejection {FieldId}", request.FieldId);
        }
    }
}
