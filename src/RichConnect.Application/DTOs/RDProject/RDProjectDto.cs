using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.RDProject
{
    public class RDProjectDto
    {
        public Guid Id { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public string BriefDescription { get; set; } = string.Empty;
        public List<string> SupportTypes { get; set; } = new();
        public string? OtherSupportType { get; set; }
        public string OrganizationResources { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string KeyDeliverables { get; set; } = string.Empty;
        public string IpConfidentialityRequirements { get; set; } = string.Empty;
        public Guid? ResearchFieldId { get; set; }
        public Guid SubmittedBy { get; set; }
        public RDProjectStatus Status { get; set; }
        public RDProjectMatchingStatus? MatchingStatus { get; set; }
        public Guid? ApprovedBy { get; set; }
        public List<Guid> MatchedFacultySpecialistIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
