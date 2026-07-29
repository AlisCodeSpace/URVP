using MediatR;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField;
using RICHConnect.Backend.Application.Commands.ResearchFields.CreateField;
using RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField;
using RICHConnect.Backend.Application.Commands.ResearchFields.RejectField;
using RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField;
using RICHConnect.Backend.Application.Queries.ResearchFields.GetAvailableFields;
using RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldById;
using RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldBySlug;
using RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldsByStatus;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.ResearchFields
{
    public class ResearchFieldApplicationService : IResearchFieldApplicationService
    {
        private readonly IMediator _mediator;
        private readonly ResearchFieldBusinessRulesService _businessRulesService;

        public ResearchFieldApplicationService(
            IMediator mediator,
            ResearchFieldBusinessRulesService businessRulesService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        // Command methods

        public async Task<ResearchFieldDto> CreateFieldAsync(CreateFieldCommand command)
        {
            // Validate business rules
            var validationResult = await _businessRulesService.ValidateFieldOperationAsync(
                Guid.Empty, // New field, no existing ID
                "create",
                command.SubmittedBy);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot create field: {string.Join(", ", validationResult.Errors)}");
            }

            // Validate name uniqueness
            if (!await _businessRulesService.ValidateNameUniquenessAsync(command.Name))
            {
                throw new InvalidOperationException("A research field with this name already exists.");
            }

            // Execute command
            return await _mediator.Send(command);
        }

        public async Task<ResearchFieldDto> UpdateFieldAsync(UpdateFieldCommand command)
        {
            // Validate business rules
            var validationResult = await _businessRulesService.ValidateFieldOperationAsync(
                command.FieldId,
                "update",
                command.UpdatedBy);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot update field: {string.Join(", ", validationResult.Errors)}");
            }

            // Validate name uniqueness (excluding current field)
            if (!await _businessRulesService.ValidateNameUniquenessAsync(command.Name, command.FieldId))
            {
                throw new InvalidOperationException("A research field with this name already exists.");
            }

            // Execute command
            return await _mediator.Send(command);
        }

        public async Task<bool> ApproveFieldAsync(ApproveFieldCommand command)
        {
            // Validate business rules
            var validationResult = await _businessRulesService.ValidateFieldOperationAsync(
                command.FieldId,
                "approve",
                command.ApprovedBy);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot approve field: {string.Join(", ", validationResult.Errors)}");
            }

            // Execute command
            return await _mediator.Send(command);
        }

        public async Task<bool> RejectFieldAsync(RejectFieldCommand command)
        {
            // Validate business rules
            var validationResult = await _businessRulesService.ValidateFieldOperationAsync(
                command.FieldId,
                "reject",
                command.RejectedBy);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot reject field: {string.Join(", ", validationResult.Errors)}");
            }

            // Execute command
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteFieldAsync(DeleteFieldCommand command)
        {
            // Validate business rules
            var validationResult = await _businessRulesService.ValidateFieldOperationAsync(
                command.FieldId,
                "delete",
                command.DeletedBy);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Cannot delete field: {string.Join(", ", validationResult.Errors)}");
            }

            // Check for dependencies
            if (await _businessRulesService.HasDependenciesAsync(command.FieldId))
            {
                throw new InvalidOperationException("Cannot delete field because it has dependencies.");
            }

            // Execute command
            return await _mediator.Send(command);
        }

        // Query methods

        public async Task<ResearchFieldDto> GetByIdAsync(Guid id)
        {
            var query = new GetFieldByIdQuery(id);
            return await _mediator.Send(query);
        }

        public async Task<ResearchFieldDto> GetBySlugAsync(string slug)
        {
            var query = new GetFieldBySlugQuery(slug);
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ResearchFieldDto>> GetAllActiveAsync()
        {
            var query = new GetFieldsByStatusQuery(ApprovalStatus.Approved);
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ResearchFieldDto>> GetAllIncludingInactiveAsync()
        {
            // Get all fields regardless of status
            var activeQuery = new GetFieldsByStatusQuery(ApprovalStatus.Approved);
            var pendingQuery = new GetFieldsByStatusQuery(ApprovalStatus.Pending);
            var rejectedQuery = new GetFieldsByStatusQuery(ApprovalStatus.Rejected);

            var activeFields = await _mediator.Send(activeQuery);
            var pendingFields = await _mediator.Send(pendingQuery);
            var rejectedFields = await _mediator.Send(rejectedQuery);

            var allFields = new List<ResearchFieldDto>();
            allFields.AddRange(activeFields);
            allFields.AddRange(pendingFields);
            allFields.AddRange(rejectedFields);

            return allFields;
        }

        public async Task<IEnumerable<ResearchFieldDto>> GetByStatusAsync(ApprovalStatus status)
        {
            var query = new GetFieldsByStatusQuery(status);
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ResearchFieldDto>> GetBySubmitterAsync(Guid userId)
        {
            // This would need a specific query implementation
            // For now, we'll get all fields and filter by submitter
            var allFields = await GetAllIncludingInactiveAsync();
            return allFields.Where(f => f.SubmittedBy == userId);
        }

        public async Task<IEnumerable<ResearchFieldDto>> GetAvailableFieldsForUserAsync(Guid userId)
        {
            var query = new GetAvailableFieldsQuery(userId);
            return await _mediator.Send(query);
        }

        // Validation methods

        public async Task<bool> CanApproveFieldAsync(Guid fieldId)
        {
            return await _businessRulesService.CanApproveFieldAsync(fieldId);
        }

        public async Task<bool> CanRejectFieldAsync(Guid fieldId)
        {
            return await _businessRulesService.CanRejectFieldAsync(fieldId);
        }

        public async Task<bool> CanDeleteFieldAsync(Guid fieldId)
        {
            return await _businessRulesService.CanDeleteFieldAsync(fieldId);
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null)
        {
            return await _businessRulesService.ValidateSlugUniquenessAsync(slug, excludeId);
        }
    }
}
