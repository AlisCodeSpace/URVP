using FEA.URVP.Application.Features.Email.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Email.Queries.List;

public sealed class ListEmailLogsQuery
    : IRequest<(IReadOnlyList<EmailLogDto> Items, int TotalCount)>
{
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListEmailLogsQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
