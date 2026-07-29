using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for creating a new challenge
    /// </summary>
    public class CreateChallengeDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        // ResearchFieldId is optional when OtherResearchFieldName is provided
        public Guid ResearchFieldId { get; set; }
        
        // Optional field for when user selects "Other" as research field
        [StringLength(128)]
        public string? OtherResearchFieldName { get; set; }
        
        [Required]
        [Range(typeof(decimal), "0.01", "1000000000")]
        public decimal EstimatedCost { get; set; }
        
        public string? SupportingDocumentUrl { get; set; }
    }
}
