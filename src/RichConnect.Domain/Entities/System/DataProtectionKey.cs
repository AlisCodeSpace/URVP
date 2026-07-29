using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Domain.Entities.System
{
    /// <summary>
    /// Entity for storing ASP.NET Core Data Protection keys
    /// Used to persist encryption keys across application restarts and multiple server instances
    /// This entity maps to the same table as Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey
    /// </summary>
    public class DataProtectionKey
    {
        /// <summary>
        /// Primary key for the data protection key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Friendly name for the key (optional)
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// XML representation of the key
        /// </summary>
        public string Xml { get; set; } = string.Empty;
    }
}
