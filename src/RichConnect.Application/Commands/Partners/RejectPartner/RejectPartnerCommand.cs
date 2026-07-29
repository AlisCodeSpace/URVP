using MediatR;

namespace RICHConnect.Backend.Application.Commands.Partners.RejectPartner
{
    /// <summary>
    /// Command for rejecting a pending community partner
    /// </summary>
    public class RejectPartnerCommand : IRequest<bool>
    {
        /// <summary>
        /// ID of the partner to reject
        /// </summary>
        public Guid PartnerId { get; set; }
        
        /// <summary>
        /// ID of the admin user performing the rejection
        /// </summary>
        public Guid AdminUserId { get; set; }
        
        /// <summary>
        /// Required reason for rejection
        /// </summary>
        public string RejectionReason { get; set; } = null!;
    }
}