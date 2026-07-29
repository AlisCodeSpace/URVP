using System.Text.Json;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Admin;
using RICHConnect.Backend.Application.Services.Partners;

namespace RICHConnect.Backend.Application.Commands.Partners.ApprovePartner
{
    /// <summary>
    /// Handler for ApprovePartnerCommand
    /// </summary>
    public class ApprovePartnerCommandHandler : BaseCommandHandler<ApprovePartnerCommand, bool>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IEventBus _eventBus;
        private readonly PartnerBusinessRulesService _businessRulesService;
        
        public ApprovePartnerCommandHandler(
            IPartnerRepository partnerRepository,
            IEventBus eventBus,
            PartnerBusinessRulesService businessRulesService,
            AppDbContext context,
            ILogger<ApprovePartnerCommandHandler> logger)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _eventBus = eventBus;
            _businessRulesService = businessRulesService;
        }

        /// <summary>
        /// Handles the command to approve a community partner
        /// </summary>
        protected override async Task<bool> HandleInternal(ApprovePartnerCommand command, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and ApprovePartnerCommandValidator
            var partner = await _partnerRepository.GetByIdAsync(command.PartnerId);
            if (partner == null)
            {
                throw new InvalidOperationException($"Partner with ID {command.PartnerId} not found.");
            }

            // Validate critical fields before approval
            var criticalFieldsValidation = _businessRulesService.ValidateCriticalFieldsForApproval(partner);
            if (!criticalFieldsValidation.IsValid)
            {
                throw new InvalidOperationException(criticalFieldsValidation.ErrorMessage ?? "Partner does not meet approval requirements.");
            }

            // Update status
            partner.Status = ApprovalStatus.Approved;
            partner.ApprovedAt = DateTime.UtcNow;
            partner.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _partnerRepository.UpdateAsync(partner);

            // Create admin action log
            var log = new AdminActionLog
            {
                AdminUserId = command.AdminUserId,
                ActionType = "ApproveCommunityPartner",
                EntityType = "CommunityPartner",
                EntityId = partner.Id,
                ClientIpHash = null,
                OldValues = JsonSerializer.Serialize(new { Status = ApprovalStatus.Pending }),
                NewValues = JsonSerializer.Serialize(new { Status = ApprovalStatus.Approved }),
                CreatedAt = DateTime.UtcNow
            };
            _context.AdminActionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            // Get user email for the event
            string partnerEmail = partner.User?.Email ?? "unknown@example.com";

            // Publish event
            var partnerApprovedEvent = new PartnerApprovedEvent(
                partner.Id,
                command.AdminUserId,
                partner.ApprovedAt.Value,
                partner.InstitutionName,
                partnerEmail);

            await _eventBus.PublishAsync(partnerApprovedEvent);

            return true;
        }
    }
}