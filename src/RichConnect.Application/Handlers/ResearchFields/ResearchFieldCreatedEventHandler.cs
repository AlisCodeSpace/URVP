using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldSubmitted;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.ResearchFields
{
    public class ResearchFieldCreatedEventHandler : IEventHandler<ResearchFieldCreatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ResearchFieldCreatedEventHandler> _logger;

        public ResearchFieldCreatedEventHandler(
            IMediator mediator,
            ILogger<ResearchFieldCreatedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(ResearchFieldCreatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ResearchFieldCreatedEvent for field {FieldId} submitted by {SubmittedBy}", 
                    domainEvent.FieldId, domainEvent.SubmittedBy);

                // Send notification to admins for review (if submitted by facultySpecialist) using CQRS
                if (domainEvent.Status == ApprovalStatus.Pending)
                {
                    await _mediator.Send(new NotifyResearchFieldSubmittedCommand
                    {
                        FieldId = domainEvent.FieldId,
                        SubmittedByUserId = domainEvent.SubmittedBy
                    });
                    
                    _logger.LogInformation("Sent notification to admins for research field {FieldId} review", domainEvent.FieldId);
                }

                // Log creation action
                _logger.LogInformation("Research field '{FieldName}' created with ID {FieldId} by user {SubmittedBy}. Status: {Status}, Active: {IsActive}", 
                    domainEvent.Name, 
                    domainEvent.FieldId, 
                    domainEvent.SubmittedBy, 
                    domainEvent.Status, 
                    domainEvent.IsActive);

                // Note: Research fields are indexed and added to catalog when approved, not when created
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResearchFieldCreatedEvent for field {FieldId}", domainEvent.FieldId);
                throw;
            }
        }
    }
}

