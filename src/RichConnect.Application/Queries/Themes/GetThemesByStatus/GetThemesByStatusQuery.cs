using MediatR;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemesByStatus
{
    public class GetThemesByStatusQuery : IRequest<List<ResearchTheme>>
    {
        public ApprovalStatus Status { get; set; }
        public Guid? UserId { get; set; } // Optional: filter by user
        public bool IncludeInactive { get; set; } = false; // Optional: include inactive themes
        public bool OnlyPublished { get; set; } = false; // Optional: filter by published status (for public endpoints)
        
        public GetThemesByStatusQuery()
        {
            // Default constructor for deserialization
        }
        
        public GetThemesByStatusQuery(ApprovalStatus status, Guid? userId = null, bool includeInactive = false, bool onlyPublished = false)
        {
            Status = status;
            UserId = userId;
            IncludeInactive = includeInactive;
            OnlyPublished = onlyPublished;
        }
    }
}
