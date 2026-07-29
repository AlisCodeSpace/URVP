using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetAvailableFields
{
    public class GetAvailableFieldsQuery : IRequest<IEnumerable<ResearchFieldDto>>
    {
        public Guid UserId { get; }

        public GetAvailableFieldsQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}

