using MediatR;
using RICHConnect.Backend.Application.DTOs.Settings;
using RICHConnect.Backend.Application.Interfaces.Settings;

namespace RICHConnect.Backend.Application.Queries.Settings.ListSettings
{
    /// <summary>
    /// Handler for ListSettingsQuery.
    /// </summary>
    public class ListSettingsQueryHandler : IRequestHandler<ListSettingsQuery, IEnumerable<AppSettingDto>>
    {
        private readonly ISettingsService _settingsService;

        public ListSettingsQueryHandler(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task<IEnumerable<AppSettingDto>> Handle(ListSettingsQuery request, CancellationToken cancellationToken)
        {
            var list = await _settingsService.ListAsync(request.IncludeSecretValues, cancellationToken);

            if (string.IsNullOrEmpty(request.Category))
                return list;

            return list.Where(s => string.Equals(s.Category, request.Category, StringComparison.OrdinalIgnoreCase));
        }
    }
}
