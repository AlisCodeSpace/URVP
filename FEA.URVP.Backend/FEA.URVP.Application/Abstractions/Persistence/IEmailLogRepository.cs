using FEA.URVP.Domain.Entities.Notifications;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IEmailLogRepository
{
    void Add(EmailLog log);

    Task<(IReadOnlyList<EmailLog> Items, int TotalCount)> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
