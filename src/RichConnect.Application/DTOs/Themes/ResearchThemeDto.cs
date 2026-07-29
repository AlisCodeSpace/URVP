// File: RICHConnect.Backend/DTOs/ThemeDto.cs

using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    /// <summary>
    /// Base theme DTO used for responses and common operations
    /// </summary>
    public class ResearchThemeDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double EstimatedFunding { get; set; }
        public ApprovalStatus Status { get; set; }
        public bool IsPublished { get; set; }
        public Guid SubmittedBy { get; set; }
        public Guid? ApprovedBy { get; set; }
        
        // Research Field relationship
        public Guid? ResearchFieldId { get; set; }
        public ResearchFieldDto? ResearchField { get; set; }
        
        // URLs returned in GET responses
        public string? ImageUrl { get; set; }
        public string? DocumentUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for facultySpecialist theme submission - includes all facultySpecialist-specific fields
    /// Now supports multiple document uploads
    /// </summary>
    public class FacultySpecialistThemeSubmissionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double EstimatedFunding { get; set; }
        public Guid? ResearchFieldId { get; set; }
        
        // Supporting document files for facultySpecialist theme submission (multiple files supported)
        public List<IFormFile>? Documents { get; set; }
        
        // Single document for backwards compatibility (deprecated, use Documents instead)
        public IFormFile? Document { get; set; }
    }

    /// <summary>
    /// DTO for Admin theme creation - includes all fields with Images
    /// Now supports multiple image uploads
    /// </summary>
    public class AdminThemeCreationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double EstimatedFunding { get; set; }
        public Guid? ResearchFieldId { get; set; }
        
        // Image files for admin theme creation (multiple files supported)
        public List<IFormFile>? Images { get; set; }
        
        // Single image for backwards compatibility (deprecated, use Images instead)
        public IFormFile? Image { get; set; }
    }

    /// <summary>
    /// DTO for Admin theme updates - allows updating existing themes
    /// Now supports multiple image uploads
    /// </summary>
    public class AdminThemeUpdateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double EstimatedFunding { get; set; }
        public Guid? ResearchFieldId { get; set; }
        
        // Image files for theme update (multiple files supported)
        public List<IFormFile>? Images { get; set; }
        
        // Single image for backwards compatibility (deprecated, use Images instead)
        public IFormFile? Image { get; set; }
    }
}
