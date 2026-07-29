using RICHConnect.Backend.Application.DTOs.Settings;

namespace RICHConnect.Backend.Application.Interfaces.Settings
{
    /// <summary>
    /// Application service for reading and writing admin-manageable settings. Secrets are encrypted at rest and masked when listing without reveal.
    /// </summary>
    public interface ISettingsService
    {
        Task<string?> GetValueAsync(string key, CancellationToken ct = default);

        Task SetAsync(string key, string value, bool isSecret, string? category, string? description, Guid? updatedBy, CancellationToken ct = default);

        Task<IReadOnlyList<AppSettingDto>> ListAsync(bool includeSecretValues, CancellationToken ct = default);

        /// <summary>
        /// Gets a single setting by key as DTO. Value is masked when IsSecret and includeSecretValue is false.
        /// </summary>
        Task<AppSettingDto?> GetByKeyAsDtoAsync(string key, bool includeSecretValue, CancellationToken ct = default);

        Task<bool> DeleteAsync(string key, CancellationToken ct = default);
    }
}
