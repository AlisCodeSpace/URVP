using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRejected;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Handlers.Partners
{
    public class PartnerRejectedEventHandler : IEventHandler<PartnerRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PartnerRejectedEventHandler> _logger;
        
        public PartnerRejectedEventHandler(
            IMediator mediator,
            IEventBus eventBus,
            ILogger<PartnerRejectedEventHandler> logger)
        {
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public async Task HandleAsync(PartnerRejectedEvent domainEvent)
        {
            try
            {
                // Send notification to partner about rejection with reason using CQRS
                await _mediator.Send(new NotifyPartnerRejectedCommand
                {
                    PartnerId = domainEvent.PartnerId,
                    RejectedByUserId = domainEvent.RejectedByAdminId,
                    RejectionReason = domainEvent.RejectionReason
                });
                
                // Log the rejection action
                _logger.LogInformation(
                    "Partner rejected: {PartnerId}, Institution: {InstitutionName}, RejectedBy: {AdminId}, Reason: {Reason}",
                    domainEvent.PartnerId,
                    domainEvent.InstitutionName,
                    domainEvent.RejectedByAdminId,
                    domainEvent.RejectionReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error handling PartnerRejectedEvent for partner {PartnerId}", 
                    domainEvent.PartnerId);
                throw;
            }
        }
    }
}

