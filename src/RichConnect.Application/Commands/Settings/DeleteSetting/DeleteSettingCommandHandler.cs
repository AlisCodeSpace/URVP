using System.Text.Json;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Settings;
using RICHConnect.Backend.Domain.Entities.Admin;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces;

namespace RICHConnect.Backend.Application.Commands.Settings.DeleteSetting
{
    /// <summary>
    /// Handler for DeleteSettingCommand. Deletes the setting and logs the action (value masked if secret).
    /// </summary>
    public class DeleteSettingCommandHandler : BaseCommandHandler<DeleteSettingCommand, bool>
    {
        private const string SecretMask = "********";

        private readonly ISettingsService _settingsService;
        private readonly ISettingsRepository _settingsRepository;

        public DeleteSettingCommandHandler(
            ISettingsService settingsService,
            ISettingsRepository settingsRepository,
            AppDbContext context,
            ILogger<DeleteSettingCommandHandler> logger)
            : base(logger, context)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        }

        protected override async Task<bool> HandleInternal(DeleteSettingCommand request, CancellationToken cancellationToken)
        {
            var key = request.Key;

            var existing = await _settingsRepository.GetByKeyAsync(key, cancellationToken);
            if (existing == null)
                return false;

            var entityId = existing.Id;
            var oldValuesJson = ToAuditJson(existing.Key, existing.Value, existing.IsSecret, existing.Category, existing.Description);

            var deleted = await _settingsService.DeleteAsync(key, cancellationToken);
            if (!deleted)
                return false;

            var log = new AdminActionLog
            {
                AdminUserId = request.AdminUserId,
                ActionType = "Delete",
                EntityType = "AppSetting",
                EntityId = entityId,
                ClientIpHash = null,
                OldValues = oldValuesJson,
                NewValues = null,
                CreatedAt = DateTime.UtcNow
            };
            _context.AdminActionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        /// <summary>
        /// Serializes setting for audit. Secrets are always masked so they never appear in AdminActionLog.
        /// </summary>
        private static string ToAuditJson(string key, string value, bool isSecret, string? category, string? description)
        {
            var displayValue = isSecret ? SecretMask : value;
            return JsonSerializer.Serialize(new { Key = key, Value = displayValue, IsSecret = isSecret, Category = category, Description = description });
        }
    }
}
