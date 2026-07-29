using MediatR;

namespace RICHConnect.Backend.Application.Commands.Settings.DeleteSetting
{
    /// <summary>
    /// Command to delete an application setting by key.
    /// </summary>
    public class DeleteSettingCommand : IRequest<bool>
    {
        public string Key { get; set; } = null!;

        /// <summary>
        /// ID of the admin user performing the delete (for audit).
        /// </summary>
        public Guid AdminUserId { get; set; }
    }
}
