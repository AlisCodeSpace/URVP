using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Faculty
{
    /// <summary>
    /// Research interest with metadata
    /// </summary>
    public class ResearchInterestDto
    {
        public string Name { get; set; } = string.Empty;
        public CreatorType CreatedBy { get; set; }
        public bool CanEdit { get; set; }
    }

    public class FacultySpecialistDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        
        [Obsolete("Use ResearchInterestsWithMetadata instead")]
        public List<string> ResearchInterests { get; set; } = new List<string>();
        
        public List<ResearchInterestDto> ResearchInterestsWithMetadata { get; set; } = new List<ResearchInterestDto>();
        
        public string CreatedAt { get; set; } = string.Empty; // ISO 8601 date string
        public string UpdatedAt { get; set; } = string.Empty; // ISO 8601 date string
    }
}
