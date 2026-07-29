using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Services.ResearchFields
{
    /// <summary>
    /// Service for enforcing business rules related to research fields
    /// </summary>
    public class ResearchFieldBusinessRulesService
    {
        private readonly IResearchFieldRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;

        public ResearchFieldBusinessRulesService(
            IResearchFieldRepository repository,
            IUserRepository userRepository,
            AppDbContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Check if a research field can be approved
        /// </summary>
        public async Task<bool> CanApproveFieldAsync(Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            return field != null && field.Status == ApprovalStatus.Pending;
        }

        /// <summary>
        /// Check if a research field can be rejected
        /// </summary>
        public async Task<bool> CanRejectFieldAsync(Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            return field != null && field.Status == ApprovalStatus.Pending;
        }

        /// <summary>
        /// Check if a research field can be deleted
        /// </summary>
        public async Task<bool> CanDeleteFieldAsync(Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            if (field == null)
                return false;

            // Check for dependencies - fields with dependencies cannot be deleted
            var hasDependencies = await HasDependenciesAsync(fieldId);
            if (hasDependencies)
                return false;

            return true;
        }

        /// <summary>
        /// Check if a user can update a research field
        /// </summary>
        public async Task<bool> CanUpdateFieldAsync(Guid fieldId, Guid userId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            if (field == null)
                return false;

            // Admin can update any field
            var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
            if (isAdmin)
                return true;

            // Faculty Specialist can update their own submitted fields
            var isFacultySpecialist = await _userRepository.HasRoleAsync(userId, UserRole.FacultySpecialist);
            if (isFacultySpecialist && field.SubmittedBy == userId)
                return true;

            return false;
        }

        /// <summary>
        /// Validate that a slug is unique
        /// </summary>
        public async Task<bool> ValidateSlugUniquenessAsync(string slug, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            var existingField = await _repository.GetBySlugAsync(slug);
            
            // If no existing field found, slug is unique
            if (existingField == null)
                return true;

            // If we're updating and it's the same field, it's still unique
            if (excludeId.HasValue && existingField.Id == excludeId.Value)
                return true;

            // Slug is not unique
            return false;
        }

        /// <summary>
        /// Check if a user can access a research field
        /// </summary>
        public async Task<bool> CanUserAccessFieldAsync(Guid userId, Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            if (field == null)
                return false;

            // Active and approved fields are accessible to all users
            if (field.IsActive && field.Status == ApprovalStatus.Approved)
                return true;

            // Users can access fields they submitted
            if (field.SubmittedBy == userId)
                return true;

            // Admins can access all fields
            var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
            if (isAdmin)
                return true;

            // Other users can only access approved and active fields
            return false;
        }

        /// <summary>
        /// Check if a research field can be activated
        /// </summary>
        public async Task<bool> CanActivateFieldAsync(Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            return field != null && field.Status == ApprovalStatus.Approved;
        }

        /// <summary>
        /// Check if a research field can be deactivated
        /// </summary>
        public async Task<bool> CanDeactivateFieldAsync(Guid fieldId)
        {
            var field = await _repository.GetByIdAsync(fieldId);
            return field != null && field.IsActive;
        }

        /// <summary>
        /// Validate that a research field name is unique
        /// </summary>
        public async Task<bool> ValidateNameUniquenessAsync(string name, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Get all fields to check for name conflicts
            var allFields = await _repository.GetAllIncludingInactiveAsync();
            
            foreach (var field in allFields)
            {
                // Skip the field we're updating
                if (excludeId.HasValue && field.Id == excludeId.Value)
                    continue;

                // Check for exact name match (case-insensitive)
                if (string.Equals(field.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if a research field has any dependencies that would prevent deletion
        /// </summary>
        public async Task<bool> HasDependenciesAsync(Guid fieldId)
        {
            // Check actual FK usage before blocking deletion.
            var hasThemeDependency = await _context.Themes
                .AsNoTracking()
                .AnyAsync(t => t.ResearchFieldId == fieldId);

            if (hasThemeDependency) return true;

            var hasChallengeDependency = await _context.Challenges
                .AsNoTracking()
                .AnyAsync(c => c.ResearchFieldId == fieldId);

            if (hasChallengeDependency) return true;

            var hasRDProjectDependency = await _context.RDProjects
                .AsNoTracking()
                .AnyAsync(p => p.ResearchFieldId == fieldId);

            if (hasRDProjectDependency) return true;

            var hasFacultySpecialistDependency = await _context.FacultySpecialistResearchFields
                .AsNoTracking()
                .AnyAsync(link => link.ResearchFieldId == fieldId);

            return hasFacultySpecialistDependency;
        }

        /// <summary>
        /// Get the business rules validation result for a field operation
        /// </summary>
        public async Task<BusinessRulesValidationResult> ValidateFieldOperationAsync(
            Guid fieldId, 
            string operation, 
            Guid userId)
        {
            var result = new BusinessRulesValidationResult
            {
                IsValid = true,
                FieldId = fieldId,
                Operation = operation,
                UserId = userId
            };

            // For create operations, we don't need to find an existing field
            if (operation.ToLowerInvariant() == "create")
            {
                // For create operations, we just validate that the user can create fields
                // This could include role-based checks, but for now we'll allow it
                return result;
            }

            // For all other operations, we need to find the existing field
            var field = await _repository.GetByIdAsync(fieldId);
            if (field == null)
            {
                result.IsValid = false;
                result.Errors.Add("Research field not found.");
                return result;
            }

            switch (operation.ToLowerInvariant())
            {
                case "approve":
                    if (!await CanApproveFieldAsync(fieldId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Field cannot be approved. Only pending fields can be approved.");
                    }
                    break;

                case "reject":
                    if (!await CanRejectFieldAsync(fieldId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Field cannot be rejected. Only pending fields can be rejected.");
                    }
                    break;

                case "delete":
                    if (!await CanDeleteFieldAsync(fieldId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Field cannot be deleted. It may have dependencies.");
                    }
                    break;

                case "update":
                    if (!await CanUpdateFieldAsync(fieldId, userId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("User does not have permission to update this field.");
                    }
                    break;

                case "activate":
                    if (!await CanActivateFieldAsync(fieldId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Field cannot be activated. Only approved fields can be activated.");
                    }
                    break;

                case "deactivate":
                    if (!await CanDeactivateFieldAsync(fieldId))
                    {
                        result.IsValid = false;
                        result.Errors.Add("Field cannot be deactivated. Field is not currently active.");
                    }
                    break;

                default:
                    result.IsValid = false;
                    result.Errors.Add($"Unknown operation: {operation}");
                    break;
            }

            return result;
        }
    }

    /// <summary>
    /// Result of business rules validation
    /// </summary>
    public class BusinessRulesValidationResult
    {
        public bool IsValid { get; set; }
        public Guid FieldId { get; set; }
        public string Operation { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public System.Collections.Generic.List<string> Errors { get; set; } = new System.Collections.Generic.List<string>();
    }
}

