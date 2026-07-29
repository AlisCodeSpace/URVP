using Microsoft.AspNetCore.DataProtection;
using RICHConnect.Backend.Application.DTOs.Settings;
using RICHConnect.Backend.Application.Interfaces.Settings;
using RICHConnect.Backend.Domain.Entities.System;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces;

namespace RICHConnect.Backend.Application.Services.Settings
{
    /// <summary>
    /// Application service for admin-manageable settings. Encrypts secrets at rest and masks them when listing without reveal.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string ProtectionPurpose = "RICHConnect.Backend.AppSettings.Secrets";
        private const string SecretMask = "********";

        private readonly ISettingsRepository _repository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(
            ISettingsRepository repository,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SettingsService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataProtectionProvider = dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string?> GetValueAsync(string key, CancellationToken ct = default)
        {
            var setting = await _repository.GetByKeyAsync(key, ct);
            if (setting == null)
                return null;

            var value = setting.Value;
            if (setting.IsSecret)
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(ProtectionPurpose);
                    value = protector.Unprotect(value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to unprotect secret setting key {Key}", key);
                    return null;
                }
            }

            return value;
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, string value, bool isSecret, string? category, string? description, Guid? updatedBy, CancellationToken ct = default)
        {
            var storedValue = value;
            if (isSecret)
            {
                var protector = _dataProtectionProvider.CreateProtector(ProtectionPurpose);
                storedValue = protector.Protect(value);
            }

            var existing = await _repository.GetByKeyAsync(key, ct);
            var now = DateTime.UtcNow;

            var setting = existing ?? new AppSetting
            {
                Key = key,
                Id = Guid.NewGuid()
            };

            setting.Value = storedValue;
            setting.IsSecret = isSecret;
            setting.Category = category;
            setting.Description = description;
            setting.UpdatedAt = now;
            setting.UpdatedBy = updatedBy;

            await _repository.UpsertAsync(setting, ct);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AppSettingDto>> ListAsync(bool includeSecretValues, CancellationToken ct = default)
        {
            var list = await _repository.GetAllAsync(ct);
            var result = new List<AppSettingDto>(list.Count);
            var protector = includeSecretValues ? _dataProtectionProvider.CreateProtector(ProtectionPurpose) : null;

            foreach (var s in list)
            {
                var value = s.Value;
                if (s.IsSecret)
                {
                    if (includeSecretValues && protector != null)
                    {
                        try
                        {
                            value = protector.Unprotect(s.Value);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to unprotect secret setting key {Key} for list", s.Key);
                            // Debugging aid: when secrets are explicitly requested, return stored raw value
                            // so admins can verify what is persisted in DB if decryption fails.
                            value = s.Value;
                        }
                    }
                    else
                    {
                        value = SecretMask;
                    }
                }

                result.Add(new AppSettingDto
                {
                    Key = s.Key,
                    Value = value,
                    IsSecret = s.IsSecret,
                    Category = s.Category,
                    Description = s.Description,
                    UpdatedAt = s.UpdatedAt,
                    UpdatedBy = s.UpdatedBy,
                    UpdatedByEmail = null
                });
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<AppSettingDto?> GetByKeyAsDtoAsync(string key, bool includeSecretValue, CancellationToken ct = default)
        {
            var setting = await _repository.GetByKeyAsync(key, ct);
            if (setting == null)
                return null;

            var value = setting.Value;
            if (setting.IsSecret)
            {
                if (includeSecretValue)
                {
                    try
                    {
                        var protector = _dataProtectionProvider.CreateProtector(ProtectionPurpose);
                        value = protector.Unprotect(setting.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to unprotect secret setting key {Key}", key);
                        // Debugging aid: when reveal=true, return stored raw value
                        // so admins can inspect persisted secret data.
                        value = setting.Value;
                    }
                }
                else
                {
                    value = SecretMask;
                }
            }

            return new AppSettingDto
            {
                Key = setting.Key,
                Value = value,
                IsSecret = setting.IsSecret,
                Category = setting.Category,
                Description = setting.Description,
                UpdatedAt = setting.UpdatedAt,
                UpdatedBy = setting.UpdatedBy,
                UpdatedByEmail = null
            };
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
        {
            return await _repository.DeleteByKeyAsync(key, ct);
        }
    }
}
