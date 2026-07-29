using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldById
{
    public class GetFieldByIdQuery : IRequest<ResearchFieldDto>
    {
        public Guid FieldId { get; }

        public GetFieldByIdQuery(Guid fieldId)
        {
            FieldId = fieldId;
        }
    }
}
