using MediatR;
using RICHConnect.Backend.Application.DTOs.Partners;

namespace RICHConnect.Backend.Application.Queries.Partners.GetPartnerById
{
    /// <summary>
    /// Query to get a community partner by ID
    /// </summary>
    public class GetPartnerByIdQuery : IRequest<CommunityPartnerDto?>
    {
        /// <summary>
        /// ID of the partner to retrieve
        /// </summary>
        public Guid PartnerId { get; set; }
        
        /// <summary>
        /// Whether to include the User entity for email
        /// </summary>
        public bool IncludeUser { get; set; } = true;
    }
}