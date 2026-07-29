using MediatR;
using RICHConnect.Backend.Application.DTOs.RDProject;

namespace RICHConnect.Backend.Application.Commands.RDProjects.CreateRDProject
{
    public class CreateRDProjectCommand : IRequest<RDProjectDto>
    {
        public string ProjectTitle { get; set; } = null!;
        public string BriefDescription { get; set; } = null!;
        public List<string> SupportTypes { get; set; } = new();
        public string? OtherSupportType { get; set; }
        public string OrganizationResources { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string KeyDeliverables { get; set; } = null!;
        public string IpConfidentialityRequirements { get; set; } = null!;
        public Guid? ResearchFieldId { get; set; }
        public Guid SubmittedBy { get; set; }

        public CreateRDProjectCommand(
            string projectTitle,
            string briefDescription,
            List<string> supportTypes,
            string? otherSupportType,
            string organizationResources,
            DateTime startDate,
            DateTime endDate,
            string keyDeliverables,
            string ipConfidentialityRequirements,
            Guid? researchFieldId,
            Guid submittedBy)
        {
            ProjectTitle = projectTitle;
            BriefDescription = briefDescription;
            SupportTypes = supportTypes;
            OtherSupportType = otherSupportType;
            OrganizationResources = organizationResources;
            StartDate = startDate;
            EndDate = endDate;
            KeyDeliverables = keyDeliverables;
            IpConfidentialityRequirements = ipConfidentialityRequirements;
            ResearchFieldId = researchFieldId;
            SubmittedBy = submittedBy;
        }
    }
}
