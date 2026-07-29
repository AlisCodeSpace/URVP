using RICHConnect.Backend.Domain.Entities.System;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces
{
    /// <summary>
    /// Repository interface for AppSetting (admin-manageable settings) operations.
    /// </summary>
    public interface ISettingsRepository
    {
        Task<AppSetting?> GetByKeyAsync(string key, CancellationToken ct = default);

        Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken ct = default);

        Task<IReadOnlyList<AppSetting>> GetByCategoryAsync(string category, CancellationToken ct = default);

        Task<AppSetting> UpsertAsync(AppSetting setting, CancellationToken ct = default);

        Task<bool> DeleteByKeyAsync(string key, CancellationToken ct = default);
    }
}
