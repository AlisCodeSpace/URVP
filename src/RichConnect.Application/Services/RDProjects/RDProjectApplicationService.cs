using MediatR;
using RICHConnect.Backend.Application.Interfaces.RDProjects;
using RICHConnect.Backend.Application.DTOs.RDProject;
using RICHConnect.Backend.Application.Commands.RDProjects.CreateRDProject;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;

namespace RICHConnect.Backend.Application.Services.RDProjects
{
    public class RDProjectApplicationService : IRDProjectApplicationService
    {
        private readonly IMediator _mediator;
        private readonly IRDProjectRepository _repository;

        public RDProjectApplicationService(IMediator mediator, IRDProjectRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        public async Task<RDProjectDto> CreateRDProjectAsync(CreateRDProjectDto dto, Guid userId)
        {
            var command = new CreateRDProjectCommand(
                dto.ProjectTitle,
                dto.BriefDescription,
                dto.SupportTypes,
                dto.OtherSupportType,
                dto.OrganizationResources,
                dto.StartDate,
                dto.EndDate,
                dto.KeyDeliverables,
                dto.IpConfidentialityRequirements,
                dto.ResearchFieldId,
                userId
            );

            return await _mediator.Send(command);
        }

        public async Task<RDProjectDto?> GetRDProjectByIdAsync(Guid id)
        {
            var project = await _repository.GetByIdWithIncludesAsync(id);
            if (project == null)
                return null;

            return new RDProjectDto
            {
                Id = project.Id,
                ProjectTitle = project.ProjectTitle,
                BriefDescription = project.BriefDescription,
                SupportTypes = project.SupportTypes.Select(st => st.SupportTypeValue).ToList(),
                OtherSupportType = project.OtherSupportType,
                OrganizationResources = project.OrganizationResources,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                KeyDeliverables = project.KeyDeliverables,
                IpConfidentialityRequirements = project.IpConfidentialityRequirements,
                ResearchFieldId = project.ResearchFieldId,
                SubmittedBy = project.SubmittedBy,
                Status = project.Status,
                MatchingStatus = project.MatchingStatus,
                ApprovedBy = project.ApprovedBy,
                MatchedFacultySpecialistIds = project.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RejectionReason = project.RejectionReason
            };
        }

        public async Task<List<RDProjectDto>> GetUserRDProjectsAsync(Guid userId)
        {
            var projects = await _repository.GetByUserAsync(userId);
            return projects.Select(p => new RDProjectDto
            {
                Id = p.Id,
                ProjectTitle = p.ProjectTitle,
                BriefDescription = p.BriefDescription,
                SupportTypes = p.SupportTypes.Select(st => st.SupportTypeValue).ToList(),
                OtherSupportType = p.OtherSupportType,
                OrganizationResources = p.OrganizationResources,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                KeyDeliverables = p.KeyDeliverables,
                IpConfidentialityRequirements = p.IpConfidentialityRequirements,
                ResearchFieldId = p.ResearchFieldId,
                SubmittedBy = p.SubmittedBy,
                Status = p.Status,
                MatchingStatus = p.MatchingStatus,
                ApprovedBy = p.ApprovedBy,
                MatchedFacultySpecialistIds = p.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RejectionReason = p.RejectionReason
            }).ToList();
        }

        public async Task<List<RDProjectDto>> GetRDProjectsByStatusAsync(Domain.Enums.RDProjectStatus status)
        {
            var projects = await _repository.GetByStatusWithIncludesAsync(status);
            return projects.Select(p => new RDProjectDto
            {
                Id = p.Id,
                ProjectTitle = p.ProjectTitle,
                BriefDescription = p.BriefDescription,
                SupportTypes = p.SupportTypes.Select(st => st.SupportTypeValue).ToList(),
                OtherSupportType = p.OtherSupportType,
                OrganizationResources = p.OrganizationResources,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                KeyDeliverables = p.KeyDeliverables,
                IpConfidentialityRequirements = p.IpConfidentialityRequirements,
                ResearchFieldId = p.ResearchFieldId,
                SubmittedBy = p.SubmittedBy,
                Status = p.Status,
                MatchingStatus = p.MatchingStatus,
                ApprovedBy = p.ApprovedBy,
                MatchedFacultySpecialistIds = p.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RejectionReason = p.RejectionReason
            }).ToList();
        }

        public async Task<RDProjectDto> ApproveRDProjectAsync(Guid id, Guid adminId)
        {
            var project = await _repository.GetByIdAsync(id);
            if (project == null)
                throw new ArgumentException("R&D project not found");

            if (project.Status != Domain.Enums.RDProjectStatus.Pending)
                throw new InvalidOperationException("Only pending R&D projects can be approved");

            project.Status = Domain.Enums.RDProjectStatus.Approved;
            project.ApprovedBy = adminId;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);

            return new RDProjectDto
            {
                Id = project.Id,
                ProjectTitle = project.ProjectTitle,
                BriefDescription = project.BriefDescription,
                SupportTypes = project.SupportTypes.Select(st => st.SupportTypeValue).ToList(),
                OtherSupportType = project.OtherSupportType,
                OrganizationResources = project.OrganizationResources,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                KeyDeliverables = project.KeyDeliverables,
                IpConfidentialityRequirements = project.IpConfidentialityRequirements,
                ResearchFieldId = project.ResearchFieldId,
                SubmittedBy = project.SubmittedBy,
                Status = project.Status,
                MatchingStatus = project.MatchingStatus,
                ApprovedBy = project.ApprovedBy,
                MatchedFacultySpecialistIds = project.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RejectionReason = project.RejectionReason
            };
        }

        public async Task<RDProjectDto> RejectRDProjectAsync(Guid id, Guid adminId, string rejectionReason)
        {
            var project = await _repository.GetByIdAsync(id);
            if (project == null)
                throw new ArgumentException("R&D project not found");

            if (project.Status != Domain.Enums.RDProjectStatus.Pending)
                throw new InvalidOperationException("Only pending R&D projects can be rejected");

            project.Status = Domain.Enums.RDProjectStatus.Rejected;
            project.RejectionReason = rejectionReason;
            project.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(project);

            return new RDProjectDto
            {
                Id = project.Id,
                ProjectTitle = project.ProjectTitle,
                BriefDescription = project.BriefDescription,
                SupportTypes = project.SupportTypes.Select(st => st.SupportTypeValue).ToList(),
                OtherSupportType = project.OtherSupportType,
                OrganizationResources = project.OrganizationResources,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                KeyDeliverables = project.KeyDeliverables,
                IpConfidentialityRequirements = project.IpConfidentialityRequirements,
                ResearchFieldId = project.ResearchFieldId,
                SubmittedBy = project.SubmittedBy,
                Status = project.Status,
                MatchingStatus = project.MatchingStatus,
                ApprovedBy = project.ApprovedBy,
                MatchedFacultySpecialistIds = project.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RejectionReason = project.RejectionReason
            };
        }
    }
}
