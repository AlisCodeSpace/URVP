using MediatR;
using RICHConnect.Backend.Application.DTOs.Settings;

namespace RICHConnect.Backend.Application.Queries.Settings.GetSettingByKey
{
    /// <summary>
    /// Query to get a single application setting by key. Secret value is masked unless RevealSecret is true.
    /// </summary>
    public class GetSettingByKeyQuery : IRequest<AppSettingDto?>
    {
        public string Key { get; set; } = null!;

        /// <summary>
        /// When true, secret values are revealed. Default false.
        /// </summary>
        public bool RevealSecret { get; set; }
    }
}
