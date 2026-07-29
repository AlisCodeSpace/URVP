using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    /// <summary>
    /// DTO for bulk creating research fields
    /// </summary>
    public class BulkCreateResearchFieldDto
    {
        [Required]
        public List<CreateResearchFieldDto> Fields { get; set; } = new();
    }
    
    /// <summary>
    /// Response for bulk create operation
    /// </summary>
    public class BulkCreateResearchFieldResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<ResearchFieldDto> CreatedFields { get; set; } = new();
        public List<string> SkippedNames { get; set; } = new();
        public List<BulkCreateError> Errors { get; set; } = new();
    }
    
    /// <summary>
    /// Error detail for bulk create operation
    /// </summary>
    public class BulkCreateError
    {
        public string Name { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for bulk delete operation
    /// </summary>
    public class BulkDeleteResearchFieldResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyDeleted { get; set; }
        public int Failed { get; set; }
        public List<string> DeletedNames { get; set; } = new();
        public List<BulkCreateError> Errors { get; set; } = new();
    }
}
