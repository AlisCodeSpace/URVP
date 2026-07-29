using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldById
{
    public class GetFieldByIdQueryHandler : IRequestHandler<GetFieldByIdQuery, ResearchFieldDto?>
    {
        private readonly IResearchFieldRepository _repository;

        public GetFieldByIdQueryHandler(
            IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ResearchFieldDto?> Handle(GetFieldByIdQuery request, CancellationToken cancellationToken)
        {
            var field = await _repository.GetByIdAsync(request.FieldId);
            
            if (field == null)
                return null;

            // Map to DTO
            return new ResearchFieldDto
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
            };
        }
    }
}

