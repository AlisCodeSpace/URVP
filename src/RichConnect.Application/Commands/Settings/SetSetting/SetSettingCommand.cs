using MediatR;
using RICHConnect.Backend.Application.DTOs.Settings;

namespace RICHConnect.Backend.Application.Commands.Settings.SetSetting
{
    /// <summary>
    /// Command to create or update an application setting.
    /// </summary>
    public class SetSettingCommand : IRequest<AppSettingDto>
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public bool IsSecret { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// ID of the admin user performing the update (for audit).
        /// </summary>
        public Guid UpdatedBy { get; set; }
    }
}
