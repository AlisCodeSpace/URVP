using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerCriticalUpdate;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Handlers.Partners
{
    public class PartnerUpdatedEventHandler : IEventHandler<PartnerUpdatedEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PartnerUpdatedEventHandler> _logger;
        
        // Fields considered critical for admin notification
        private readonly string[] _criticalFields = new[] { 
            "InstitutionName", "ChamberOfCommerceNumber", "Sector", "AccreditationType" 
        };
        
        public PartnerUpdatedEventHandler(
            IMediator mediator,
            IEventBus eventBus,
            ILogger<PartnerUpdatedEventHandler> logger)
        {
            _mediator = mediator;
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public async Task HandleAsync(PartnerUpdatedEvent domainEvent)
        {
            try
            {
                // Log the update action
                _logger.LogInformation(
                    "Partner updated: {PartnerId}, UpdatedBy: {UserId}, ChangedFields: {ChangedFields}",
                    domainEvent.PartnerId,
                    domainEvent.UpdatedByUserId,
                    string.Join(", ", domainEvent.ChangedFields.Keys));
                
                // Check if any critical fields were changed
                var criticalFieldsChanged = domainEvent.ChangedFields.Keys
                    .Intersect(_criticalFields)
                    .ToList();
                
                if (criticalFieldsChanged.Any())
                {
                    // Notify admins about critical field changes using CQRS
                    await _mediator.Send(new NotifyPartnerCriticalUpdateCommand
                    {
                        PartnerId = domainEvent.PartnerId,
                        UpdatedByUserId = domainEvent.UpdatedByUserId,
                        CriticalFieldsChanged = criticalFieldsChanged
                    });
                    
                    _logger.LogInformation(
                        "Admin notification sent for critical field changes: {PartnerId}, Fields: {Fields}",
                        domainEvent.PartnerId,
                        string.Join(", ", criticalFieldsChanged));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error handling PartnerUpdatedEvent for partner {PartnerId}", 
                    domainEvent.PartnerId);
                throw;
            }
        }
    }
}

