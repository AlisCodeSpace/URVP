using MediatR;

namespace RICHConnect.Backend.Application.Commands.Partners.ApprovePartner
{
    /// <summary>
    /// Command for approving a pending community partner
    /// </summary>
    public class ApprovePartnerCommand : IRequest<bool>
    {
        /// <summary>
        /// ID of the partner to approve
        /// </summary>
        public Guid PartnerId { get; set; }
        
        /// <summary>
        /// ID of the admin user performing the approval
        /// </summary>
        public Guid AdminUserId { get; set; }
    }
}