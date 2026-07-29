using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetAvailableFields
{
    public class GetAvailableFieldsQueryHandler : IRequestHandler<GetAvailableFieldsQuery, IEnumerable<ResearchFieldDto>>
    {
        private readonly IResearchFieldRepository _repository;

        public GetAvailableFieldsQueryHandler(
            IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<ResearchFieldDto>> Handle(GetAvailableFieldsQuery request, CancellationToken cancellationToken)
        {
            var fields = await _repository.GetAvailableFieldsForUserAsync(request.UserId);
            
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
                CanEdit = field.CreatedBy == Domain.Enums.CreatorType.Faculty && field.SubmittedBy == request.UserId
            });
        }
    }
}

