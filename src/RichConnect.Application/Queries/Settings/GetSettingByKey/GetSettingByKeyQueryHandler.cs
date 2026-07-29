using MediatR;
using RICHConnect.Backend.Application.DTOs.Settings;
using RICHConnect.Backend.Application.Interfaces.Settings;

namespace RICHConnect.Backend.Application.Queries.Settings.GetSettingByKey
{
    /// <summary>
    /// Handler for GetSettingByKeyQuery. Returns DTO with masked value when secret and RevealSecret is false.
    /// </summary>
    public class GetSettingByKeyQueryHandler : IRequestHandler<GetSettingByKeyQuery, AppSettingDto?>
    {
        private readonly ISettingsService _settingsService;

        public GetSettingByKeyQueryHandler(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task<AppSettingDto?> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
        {
            return await _settingsService.GetByKeyAsDtoAsync(request.Key, request.RevealSecret, cancellationToken);
        }
    }
}
