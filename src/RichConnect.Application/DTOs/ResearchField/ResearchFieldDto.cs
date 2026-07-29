using System.ComponentModel.DataAnnotations;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    /// <summary>
    /// Base ResearchField DTO used for responses and common operations
    /// </summary>
    public class ResearchFieldDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public ApprovalStatus Status { get; set; }
        public CreatorType CreatedBy { get; set; }
        public Guid SubmittedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Indicates if the current user can edit this field (set by controller)
        /// </summary>
        public bool CanEdit { get; set; }
    }

    /// <summary>
    /// DTO for creating a new ResearchField
    /// </summary>
    public class CreateResearchFieldDto
    {
        [Required]
        public string Name { get; set; } = null!;
        
        public string? Category { get; set; }
        
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing ResearchField
    /// </summary>
    public class UpdateResearchFieldDto
    {
        [Required]
        public string Name { get; set; } = null!;
        
        public string? Category { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; }
    }
    
    /// <summary>
    /// DTO for facultySpecialist research field submission
    /// </summary>
    public class FacultySpecialistResearchFieldSubmissionDto
    {
        [Required]
        public string Name { get; set; } = null!;
        
        public string? Category { get; set; }
    }
    
    /// <summary>
    /// DTO for rejecting a research field
    /// </summary>
    public class RejectResearchFieldDto
    {
        [Required]
        public string RejectionReason { get; set; } = null!;
    }
}
