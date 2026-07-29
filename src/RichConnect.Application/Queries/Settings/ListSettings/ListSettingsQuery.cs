using MediatR;
using RICHConnect.Backend.Application.DTOs.Settings;

namespace RICHConnect.Backend.Application.Queries.Settings.ListSettings
{
    /// <summary>
    /// Query to list application settings, optionally filtered by category. Secret values are masked unless requested.
    /// </summary>
    public class ListSettingsQuery : IRequest<IEnumerable<AppSettingDto>>
    {
        /// <summary>
        /// Optional category filter (e.g. "SMTP", "Azure", "FeatureFlags").
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// When true, secret values are revealed in the response. Default false.
        /// </summary>
        public bool IncludeSecretValues { get; set; }
    }
}
