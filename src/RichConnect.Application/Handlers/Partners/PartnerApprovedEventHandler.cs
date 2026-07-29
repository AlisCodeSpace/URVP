using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerApproved;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Handlers.Partners
{
    public class PartnerApprovedEventHandler : IEventHandler<PartnerApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PartnerApprovedEventHandler> _logger;
        
        public PartnerApprovedEventHandler(
            IMediator mediator,
            IEventBus eventBus,
            ILogger<PartnerApprovedEventHandler> logger)
        {
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public async Task HandleAsync(PartnerApprovedEvent domainEvent)
        {
            try
            {
                // Send notification to partner about approval using CQRS
                await _mediator.Send(new NotifyPartnerApprovedCommand
                {
                    PartnerId = domainEvent.PartnerId,
                    ApprovedByUserId = domainEvent.ApprovedByAdminId
                });
                
                // Log the approval action
                _logger.LogInformation(
                    "Partner approved: {PartnerId}, Institution: {InstitutionName}, ApprovedBy: {AdminId}, ApprovedAt: {ApprovedAt}",
                    domainEvent.PartnerId,
                    domainEvent.InstitutionName,
                    domainEvent.ApprovedByAdminId,
                    domainEvent.ApprovedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error handling PartnerApprovedEvent for partner {PartnerId}", 
                    domainEvent.PartnerId);
                throw;
            }
        }
    }
}

