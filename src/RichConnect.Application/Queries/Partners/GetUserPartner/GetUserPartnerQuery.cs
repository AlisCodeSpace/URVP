using MediatR;
using RICHConnect.Backend.Application.DTOs.Partners;

namespace RICHConnect.Backend.Application.Queries.Partners.GetUserPartner
{
    /// <summary>
    /// Query to get a community partner by user ID
    /// </summary>
    public class GetUserPartnerQuery : IRequest<CommunityPartnerDto?>
    {
        /// <summary>
        /// ID of the user to get the partner for
        /// </summary>
        public Guid UserId { get; set; }
    }
}