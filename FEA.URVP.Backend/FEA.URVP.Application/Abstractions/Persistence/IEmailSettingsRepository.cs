using FEA.URVP.Domain.Entities.Email;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IEmailSettingsRepository
{
    Task<EmailSettings?> GetAsync(CancellationToken cancellationToken = default);

    void Add(EmailSettings settings);
}
