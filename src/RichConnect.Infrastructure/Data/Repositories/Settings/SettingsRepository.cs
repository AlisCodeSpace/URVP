using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Domain.Entities.System;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Settings
{
    /// <summary>
    /// Repository implementation for AppSetting persistence.
    /// </summary>
    public class SettingsRepository : ISettingsRepository
    {
        private readonly AppDbContext _context;

        public SettingsRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken ct = default)
        {
            return await _context.AppSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key, ct);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.AppSettings
                .AsNoTracking()
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AppSetting>> GetByCategoryAsync(string category, CancellationToken ct = default)
        {
            return await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.Category == category)
                .OrderBy(s => s.Key)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<AppSetting> UpsertAsync(AppSetting setting, CancellationToken ct = default)
        {
            var existing = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == setting.Key, ct);

            if (existing != null)
            {
                existing.Value = setting.Value;
                existing.IsSecret = setting.IsSecret;
                existing.Category = setting.Category;
                existing.Description = setting.Description;
                existing.UpdatedAt = setting.UpdatedAt;
                existing.UpdatedBy = setting.UpdatedBy;
                await _context.SaveChangesAsync(ct);
                return existing;
            }

            _context.AppSettings.Add(setting);
            await _context.SaveChangesAsync(ct);
            return setting;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByKeyAsync(string key, CancellationToken ct = default)
        {
            var existing = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.Key == key, ct);

            if (existing == null)
                return false;

            _context.AppSettings.Remove(existing);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
