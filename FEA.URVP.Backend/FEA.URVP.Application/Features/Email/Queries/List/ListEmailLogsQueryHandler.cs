using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Email.DTOs;
using FEA.URVP.Application.Features.Email.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Email.Queries.List;

public sealed class ListEmailLogsQueryHandler
    : IRequestHandler<ListEmailLogsQuery, (IReadOnlyList<EmailLogDto> Items, int TotalCount)>
{
    private readonly IEmailLogRepository _logs;

    public ListEmailLogsQueryHandler(IEmailLogRepository logs)
    {
        _logs = logs;
    }

    public async Task<(IReadOnlyList<EmailLogDto> Items, int TotalCount)> Handle(
        ListEmailLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _logs.ListAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
