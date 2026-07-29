using MediatR;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Queries.Partners.GetPartnersByStatus
{
    /// <summary>
    /// Query to get community partners by status
    /// </summary>
    public class GetPartnersByStatusQuery : IRequest<List<CommunityPartnerDto>>
    {
        /// <summary>
        /// Status to filter by (null = all)
        /// </summary>
        public ApprovalStatus? Status { get; set; }
        
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; } = 50;
        
        /// <summary>
        /// Property to sort by
        /// </summary>
        public string? SortBy { get; set; } = "SubmittedAt";
        
        /// <summary>
        /// Whether to sort in descending order
        /// </summary>
        public bool SortDescending { get; set; } = true;
    }
}