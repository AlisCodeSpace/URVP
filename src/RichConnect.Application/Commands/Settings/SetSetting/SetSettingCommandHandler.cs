using System.Text.Json;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.DTOs.Settings;
using RICHConnect.Backend.Application.Interfaces.Settings;
using RICHConnect.Backend.Domain.Entities.Admin;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces;

namespace RICHConnect.Backend.Application.Commands.Settings.SetSetting
{
    /// <summary>
    /// Handler for SetSettingCommand. Creates or updates a setting and logs the action (values masked for secrets).
    /// </summary>
    public class SetSettingCommandHandler : BaseCommandHandler<SetSettingCommand, AppSettingDto>
    {
        private const string SecretMask = "********";

        private readonly ISettingsService _settingsService;
        private readonly ISettingsRepository _settingsRepository;

        public SetSettingCommandHandler(
            ISettingsService settingsService,
            ISettingsRepository settingsRepository,
            AppDbContext context,
            ILogger<SetSettingCommandHandler> logger)
            : base(logger, context)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        }

        protected override async Task<AppSettingDto> HandleInternal(SetSettingCommand request, CancellationToken cancellationToken)
        {
            var key = request.Key;

            // Load existing for audit (OldValues, and to know Create vs Update)
            var existing = await _settingsRepository.GetByKeyAsync(key, cancellationToken);
            var actionType = existing == null ? "Create" : "Update";
            var oldValuesJson = existing != null ? ToAuditJson(existing.Key, existing.Value, existing.IsSecret, existing.Category, existing.Description) : null;

            await _settingsService.SetAsync(
                key,
                request.Value,
                request.IsSecret,
                request.Category,
                request.Description,
                request.UpdatedBy,
                cancellationToken);

            var settingAfter = await _settingsRepository.GetByKeyAsync(key, cancellationToken);
            if (settingAfter == null)
                throw new InvalidOperationException("Setting was not persisted.");

            var newValuesJson = ToAuditJson(settingAfter.Key, settingAfter.Value, settingAfter.IsSecret, settingAfter.Category, settingAfter.Description);

            var log = new AdminActionLog
            {
                AdminUserId = request.UpdatedBy,
                ActionType = actionType,
                EntityType = "AppSetting",
                EntityId = settingAfter.Id,
                ClientIpHash = null,
                OldValues = oldValuesJson,
                NewValues = newValuesJson,
                CreatedAt = DateTime.UtcNow
            };
            _context.AdminActionLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            // Return DTO with value masked when secret
            var dto = await _settingsService.GetByKeyAsDtoAsync(key, includeSecretValue: false, cancellationToken);
            return dto!;
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
