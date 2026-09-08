using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Email;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class EmailSettingsRepository : IEmailSettingsRepository
{
    private readonly AppDbContext _db;

    public EmailSettingsRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<EmailSettings?> GetAsync(CancellationToken cancellationToken = default) =>
        _db.EmailSettings.FirstOrDefaultAsync(
            x => x.Id == EmailSettings.SingletonId,
            cancellationToken);

    public void Add(EmailSettings settings) => _db.EmailSettings.Add(settings);
}
