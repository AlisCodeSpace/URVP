using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for updating an existing challenge
    /// </summary>
    public class UpdateChallengeDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [Required]
        public Guid ResearchFieldId { get; set; }
        
        [Required]
        [Range(typeof(decimal), "0.01", "1000000000")]
        public decimal EstimatedCost { get; set; }
        
        public string? SupportingDocumentUrl { get; set; }
    }
}
