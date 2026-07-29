using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRegistered;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Handlers.Partners
{
    public class PartnerRegisteredEventHandler : IEventHandler<PartnerRegisteredEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PartnerRegisteredEventHandler> _logger;
        
        public PartnerRegisteredEventHandler(
            IMediator mediator,
            IEventBus eventBus,
            ILogger<PartnerRegisteredEventHandler> logger)
        {
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public async Task HandleAsync(PartnerRegisteredEvent domainEvent)
        {
            try
            {
                // Send notification to admins about new partner registration using CQRS
                await _mediator.Send(new NotifyPartnerRegisteredCommand 
                { 
                    PartnerId = domainEvent.PartnerId 
                });
                
                // Log the event
                _logger.LogInformation(
                    "Partner registered: {PartnerId}, Institution: {InstitutionName}, User: {UserId}",
                    domainEvent.PartnerId,
                    domainEvent.InstitutionName,
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error handling PartnerRegisteredEvent for partner {PartnerId}", 
                    domainEvent.PartnerId);
                throw;
            }
        }
    }
}

