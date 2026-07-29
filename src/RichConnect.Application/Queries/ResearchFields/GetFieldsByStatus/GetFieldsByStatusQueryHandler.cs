using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldsByStatus
{
    public class GetFieldsByStatusQueryHandler : IRequestHandler<GetFieldsByStatusQuery, IEnumerable<ResearchFieldDto>>
    {
        private readonly IResearchFieldRepository _repository;

        public GetFieldsByStatusQueryHandler(
            IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<ResearchFieldDto>> Handle(GetFieldsByStatusQuery request, CancellationToken cancellationToken)
        {
            var fields = await _repository.GetByStatusAsync(request.Status);
            
            // Apply pagination if requested
            if (request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                fields = fields
                    .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                    .Take(request.PageSize.Value);
            }
            
            // Map to DTOs
            return fields.Select(field => new ResearchFieldDto
            {
                Id = field.Id,
                Name = field.Name,
                Slug = field.Slug,
                Category = field.Category,
                DisplayOrder = field.DisplayOrder,
                IsActive = field.IsActive,
                Status = field.Status,
                CreatedBy = field.CreatedBy,
                SubmittedBy = field.SubmittedBy,
                CreatedAt = field.CreatedAt,
                UpdatedAt = field.UpdatedAt,
                CanEdit = false // Set by controller based on user context
            });
        }
    }
}

