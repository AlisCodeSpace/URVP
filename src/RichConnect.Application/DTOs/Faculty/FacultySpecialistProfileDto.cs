namespace RICHConnect.Backend.Application.DTOs.Faculty
{
    public class FacultySpecialistProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int Status { get; set; } // 0 = Unavailable, 1 = Available
        public string? ProfilePhoto { get; set; } // URL to profile image
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty; // Values: "computer-science", "electrical-engineering", etc.
        public string AcademicRank { get; set; } = string.Empty; // Values: "assistant-facultySpecialist", "associate-facultySpecialist", etc.
        public string? OfficeLocation { get; set; }
        public string? Biography { get; set; }
        public List<string> ResearchInterests { get; set; } = new List<string>();

        /// <summary>
        /// Research interests enriched with metadata (e.g. who created/submitted the field and whether the current user can edit it).
        /// </summary>
        public List<ResearchInterestDto> ResearchInterestsWithMetadata { get; set; } = new List<ResearchInterestDto>();
        public string CreatedAt { get; set; } = string.Empty; // ISO 8601 date string
        public string UpdatedAt { get; set; } = string.Empty; // ISO 8601 date string
        
        // FMIS Integration Fields (from FacultyMemberLite)
        public string? FmisMemberId { get; set; }
        public string? FmisRank { get; set; }
        public string? FmisDepartment { get; set; }
        public string? FmisFaculty { get; set; }
        public string? FmisLastSyncedAt { get; set; } // ISO 8601 date string
    }
} 