using System.Text.Json;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Admin;

namespace RICHConnect.Backend.Application.Commands.Partners.RejectPartner
{
    /// <summary>
    /// Handler for RejectPartnerCommand
    /// </summary>
    public class RejectPartnerCommandHandler : BaseCommandHandler<RejectPartnerCommand, bool>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IEventBus _eventBus;
        public RejectPartnerCommandHandler(
            IPartnerRepository partnerRepository,
            IEventBus eventBus,
            AppDbContext context,
            ILogger<RejectPartnerCommandHandler> logger)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _eventBus = eventBus;
        }

        /// <summary>
        /// Handles the command to reject a community partner
        /// </summary>
        protected override async Task<bool> HandleInternal(RejectPartnerCommand command, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and RejectPartnerCommandValidator
            var partner = await _partnerRepository.GetByIdAsync(command.PartnerId);
            if (partner == null)
            {
                throw new InvalidOperationException($"Partner with ID {command.PartnerId} not found.");
            }

            // Update status
            partner.Status = ApprovalStatus.Rejected;
            partner.RejectedAt = DateTime.UtcNow;
            partner.RejectionReason = command.RejectionReason.Trim();
            partner.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _partnerRepository.UpdateAsync(partner);

            // Create admin action log
            var log = new AdminActionLog
            {
                AdminUserId = command.AdminUserId,
                ActionType = "RejectCommunityPartner",
                EntityType = "CommunityPartner",
                EntityId = partner.Id,
                ClientIpHash = null,
                OldValues = JsonSerializer.Serialize(new { Status = ApprovalStatus.Pending }),
                NewValues = JsonSerializer.Serialize(new { 
                    Status = ApprovalStatus.Rejected,
                    RejectionReason = command.RejectionReason
                }),
                CreatedAt = DateTime.UtcNow
            };
            _context.AdminActionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            // Get user email for the event
            string partnerEmail = partner.User?.Email ?? "unknown@example.com";

            // Publish event
            var partnerRejectedEvent = new PartnerRejectedEvent(
                partner.Id,
                command.AdminUserId,
                command.RejectionReason,
                partner.RejectedAt.Value,
                partner.InstitutionName,
                partnerEmail);

            await _eventBus.PublishAsync(partnerRejectedEvent);

            return true;
        }
    }
}