using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class EmailLogRepository : IEmailLogRepository
{
    private readonly AppDbContext _db;

    public EmailLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Add(EmailLog log) => _db.EmailLogs.Add(log);

    public async Task<(IReadOnlyList<EmailLog> Items, int TotalCount)> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.EmailLogs.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
