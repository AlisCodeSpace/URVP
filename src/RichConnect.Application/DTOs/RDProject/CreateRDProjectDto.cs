using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RICHConnect.Backend.Application.DTOs.RDProject
{
    public class CreateRDProjectDto
    {
        [Required, StringLength(200)]
        public string ProjectTitle { get; set; } = string.Empty;
        
        [Required]
        public string BriefDescription { get; set; } = string.Empty;
        
        [Required]
        public List<string> SupportTypes { get; set; } = new();
        
        [StringLength(500)]
        public string? OtherSupportType { get; set; }
        
        [Required, StringLength(1000)]
        public string OrganizationResources { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public string KeyDeliverables { get; set; } = string.Empty;
        
        [Required]
        public string IpConfidentialityRequirements { get; set; } = string.Empty;
        
        public Guid? ResearchFieldId { get; set; }
        
        // Supporting documents for R&D project submission (multiple files supported)
        public List<IFormFile>? SupportingDocuments { get; set; }
    }
}
