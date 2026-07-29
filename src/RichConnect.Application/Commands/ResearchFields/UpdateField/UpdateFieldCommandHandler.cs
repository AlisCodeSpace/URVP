using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField
{
    public class UpdateFieldCommandHandler : BaseCommandHandler<UpdateFieldCommand, ResearchFieldDto>
    {
        private readonly IResearchFieldRepository _repository;
        private readonly IEventBus _eventBus;

        public UpdateFieldCommandHandler(
            ILogger<UpdateFieldCommandHandler> logger,
            AppDbContext context,
            IResearchFieldRepository repository,
            IEventBus eventBus)
            : base(logger, context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected override async Task<ResearchFieldDto> HandleInternal(UpdateFieldCommand request, CancellationToken cancellationToken)
        {
            // Get existing field (validation already handled by base class)
            var field = await _repository.GetByIdAsync(request.FieldId);
            if (field == null)
            {
                throw new InvalidOperationException($"Research field with ID {request.FieldId} not found.");
            }
            
            // Track changes for domain event
            var changes = new Dictionary<string, object>();
            
            // Update basic properties
            if (field.Name != request.Name.Trim())
            {
                changes["Name"] = new { Old = field.Name, New = request.Name.Trim() };
                field.Name = request.Name.Trim();
                
                // Update slug when name changes
                var oldSlug = field.Slug;
                field.Slug = GenerateSlug(request.Name.Trim());
                changes["Slug"] = new { Old = oldSlug, New = field.Slug };
            }
            
            if (field.Category != request.Category?.Trim())
            {
                changes["Category"] = new { Old = field.Category, New = request.Category?.Trim() };
                field.Category = request.Category?.Trim();
            }
            
            if (field.DisplayOrder != request.DisplayOrder)
            {
                changes["DisplayOrder"] = new { Old = field.DisplayOrder, New = request.DisplayOrder };
                field.DisplayOrder = request.DisplayOrder;
            }
            
            if (field.IsActive != request.IsActive)
            {
                changes["IsActive"] = new { Old = field.IsActive, New = request.IsActive };
                field.IsActive = request.IsActive;
            }
            
            // Update timestamp
            field.UpdatedAt = DateTime.UtcNow;
            
            // Save changes
            var updatedField = await _repository.UpdateAsync(field);
            if (updatedField == null)
            {
                throw new InvalidOperationException($"Failed to update research field with ID {request.FieldId}.");
            }
            
            // Publish domain event if there were changes
            if (changes.Count > 0)
            {
                await _eventBus.PublishAsync(new ResearchFieldUpdatedEvent(
                    updatedField.Id,
                    request.UpdatedBy,
                    changes
                ));
            }

            // Return DTO
            return new ResearchFieldDto
            {
                Id = updatedField.Id,
                Name = updatedField.Name,
                Slug = updatedField.Slug,
                Category = updatedField.Category ?? string.Empty,
                DisplayOrder = updatedField.DisplayOrder,
                IsActive = updatedField.IsActive,
                Status = updatedField.Status,
                SubmittedBy = updatedField.SubmittedBy,
                CreatedAt = updatedField.CreatedAt,
                UpdatedAt = updatedField.UpdatedAt
            };
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("+", "plus")
                .Replace("=", "equals")
                .Replace("@", "at")
                .Replace("#", "hash")
                .Replace("$", "dollar")
                .Replace("%", "percent")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("_", "-");

            // Remove multiple consecutive hyphens
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }
    }
}

