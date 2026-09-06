using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Infrastructure.Data.Context;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class EmailLogRepository : IEmailLogRepository
{
    private readonly AppDbContext _db;

    public EmailLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Add(EmailLog log) => _db.EmailLogs.Add(log);
}
