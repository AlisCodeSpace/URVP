using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldsByStatus
{
    public class GetFieldsByStatusQuery : IRequest<IEnumerable<ResearchFieldDto>>
    {
        public ApprovalStatus Status { get; }
        public int? PageNumber { get; }
        public int? PageSize { get; }

        public GetFieldsByStatusQuery(ApprovalStatus status, int? pageNumber = null, int? pageSize = null)
        {
            Status = status;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}

