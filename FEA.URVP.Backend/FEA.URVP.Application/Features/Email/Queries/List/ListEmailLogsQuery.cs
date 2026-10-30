using FEA.URVP.Application.DTOs.Email;
using MediatR;

namespace FEA.URVP.Application.Queries.EmailLogs.List;

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
