using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField
{
    public class ApproveFieldCommandHandler : BaseCommandHandler<ApproveFieldCommand, bool>
    {
        private readonly IResearchFieldRepository _repository;
        private readonly IEventBus _eventBus;
        public ApproveFieldCommandHandler(
            ILogger<ApproveFieldCommandHandler> logger,
            AppDbContext context,
            IResearchFieldRepository repository,
            IEventBus eventBus)
            : base(logger, context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected override async Task<bool> HandleInternal(ApproveFieldCommand request, CancellationToken cancellationToken)
        {
            // Get the field to approve (validation already handled by base class)
            var field = await _repository.GetByIdAsync(request.FieldId);
            if (field == null)
            {
                throw new InvalidOperationException($"Research field with ID {request.FieldId} not found.");
            }
            
            // Update field properties
            field.Status = ApprovalStatus.Approved;
            field.IsActive = true; // Activate the field when approved
            field.UpdatedAt = DateTime.UtcNow;
            
            // Save changes
            var updated = await _repository.UpdateAsync(field);
            if (updated == null)
            {
                return false;
            }
            
            // Publish domain event
            await _eventBus.PublishAsync(new ResearchFieldApprovedEvent(
                field.Id,
                request.ApprovedBy
            ));
            
            return true;
        }
    }
}

