using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.RDProject;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.RDProjects;

namespace RICHConnect.Backend.Application.Commands.RDProjects.CreateRDProject
{
    public class CreateRDProjectCommandHandler : BaseCommandHandler<CreateRDProjectCommand, RDProjectDto>
    {
        private readonly IRDProjectRepository _repository;
        private readonly IEventBus _eventBus;
        private RDProjectSubmittedEvent? _pendingDomainEvent;

        public CreateRDProjectCommandHandler(
            IRDProjectRepository repository,
            IEventBus eventBus,
            ILogger<CreateRDProjectCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _eventBus = eventBus;
        }
        
        protected override bool UseTransaction => true;

        protected override async Task<RDProjectDto> HandleInternal(CreateRDProjectCommand command, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            var rdProject = new RDProject
            {
                ProjectTitle = command.ProjectTitle.Trim(),
                BriefDescription = command.BriefDescription.Trim(),
                OrganizationResources = command.OrganizationResources.Trim(),
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                KeyDeliverables = command.KeyDeliverables.Trim(),
                IpConfidentialityRequirements = command.IpConfidentialityRequirements.Trim(),
                OtherSupportType = command.OtherSupportType?.Trim(),
                ResearchFieldId = command.ResearchFieldId,
                SubmittedBy = command.SubmittedBy,
                Status = RDProjectStatus.Pending,
                MatchingStatus = RDProjectMatchingStatus.NoInvite,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add support types as child entities
            foreach (var supportType in command.SupportTypes)
            {
                rdProject.SupportTypes.Add(new RDProjectSupportType
                {
                    SupportTypeValue = supportType
                });
            }

            var createdProject = await _repository.CreateAsync(rdProject);
            
            // Get user details for event
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.SubmittedBy, cancellationToken);
            
            // Queue domain event to be published after transaction commit
            _pendingDomainEvent = new RDProjectSubmittedEvent(
                createdProject.Id,
                command.SubmittedBy,
                createdProject.ProjectTitle,
                user?.Name ?? "Unknown User"
            );
            
            return MapToDto(createdProject);
        }

        public override async Task<RDProjectDto> Handle(CreateRDProjectCommand request, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            try
            {
                var response = await base.Handle(request, cancellationToken);
                return response;
            }
            finally
            {
                if (_pendingDomainEvent != null)
                {
                    try
                    {
                        await _eventBus.PublishAsync(_pendingDomainEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish RDProjectSubmittedEvent for RD project {RDProjectId}", _pendingDomainEvent.RDProjectId);
                    }
                    finally
                    {
                        _pendingDomainEvent = null;
                    }
                }
            }
        }

        private RDProjectDto MapToDto(RDProject project)
        {
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
