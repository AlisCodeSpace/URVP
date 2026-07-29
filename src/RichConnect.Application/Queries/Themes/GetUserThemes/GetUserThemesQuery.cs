using MediatR;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetUserThemes
{
    public class GetUserThemesQuery : IRequest<List<ResearchTheme>>
    {
        public Guid UserId { get; set; }
        public ApprovalStatus? Status { get; set; } // Optional: filter by status
        public bool IncludeInactive { get; set; } = false; // Optional: include inactive themes
        
        public GetUserThemesQuery()
        {
            // Default constructor for deserialization
        }
        
        public GetUserThemesQuery(Guid userId, ApprovalStatus? status = null, bool includeInactive = false)
        {
            UserId = userId;
            Status = status;
            IncludeInactive = includeInactive;
        }
    }
}

