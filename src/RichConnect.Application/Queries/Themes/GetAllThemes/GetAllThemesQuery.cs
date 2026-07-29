using MediatR;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetAllThemes
{
    public class GetAllThemesQuery : IRequest<List<ResearchTheme>>
    {
        public ApprovalStatus? Status { get; set; } // Optional: filter by status
        public Guid? UserId { get; set; } // Optional: filter by user
        public Guid? ResearchFieldId { get; set; } // Optional: filter by research field
        public bool IncludeInactive { get; set; } = false; // Optional: include inactive themes
        public string? SearchTerm { get; set; } // Optional: search by title
        public DateTime? FromDate { get; set; } // Optional: filter by date range
        public DateTime? ToDate { get; set; } // Optional: filter by date range
        
        public GetAllThemesQuery()
        {
            // Default constructor for deserialization
        }
        
        public GetAllThemesQuery(
            ApprovalStatus? status = null, 
            Guid? userId = null, 
            Guid? researchFieldId = null, 
            bool includeInactive = false,
            string? searchTerm = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            Status = status;
            UserId = userId;
            ResearchFieldId = researchFieldId;
            IncludeInactive = includeInactive;
            SearchTerm = searchTerm;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }
}
