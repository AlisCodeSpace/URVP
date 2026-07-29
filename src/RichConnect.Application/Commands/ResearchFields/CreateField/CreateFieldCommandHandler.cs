using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.CreateField
{
    public class CreateFieldCommandHandler : BaseCommandHandler<CreateFieldCommand, ResearchFieldDto>
    {
        private readonly IResearchFieldRepository _repository;
        private readonly IEventBus _eventBus;

        public CreateFieldCommandHandler(
            ILogger<CreateFieldCommandHandler> logger,
            AppDbContext context,
            IResearchFieldRepository repository,
            IEventBus eventBus)
            : base(logger, context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected override async Task<ResearchFieldDto> HandleInternal(CreateFieldCommand request, CancellationToken cancellationToken)
        {
            // Generate slug from name
            string slug = GenerateSlug(request.Name.Trim());

            // Set approval status based on creator role
            var status = request.IsAdminCreated ? ApprovalStatus.Approved : ApprovalStatus.Pending;
            
            // Create the research field entity
            var field = new ResearchField
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Slug = slug,
                Category = request.Category?.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsAdminCreated ? request.IsActive : false, // Only active if admin created
                Status = status,
                SubmittedBy = request.SubmittedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            var createdField = await _repository.AddAsync(field);

            // Publish domain event
            await _eventBus.PublishAsync(new ResearchFieldCreatedEvent(
                createdField.Id,
                createdField.Name,
                createdField.SubmittedBy,
                createdField.Status,
                createdField.IsActive
            ));

            // Return DTO
            return new ResearchFieldDto
            {
                Id = createdField.Id,
                Name = createdField.Name,
                Slug = createdField.Slug,
                Category = createdField.Category,
                DisplayOrder = createdField.DisplayOrder,
                IsActive = createdField.IsActive,
                Status = createdField.Status,
                SubmittedBy = createdField.SubmittedBy,
                CreatedAt = createdField.CreatedAt,
                UpdatedAt = createdField.UpdatedAt
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

