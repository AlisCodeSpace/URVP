using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.System
{
    /// <summary>
    /// Admin-manageable application setting (configuration or secret). Secrets are encrypted at the application layer.
    /// </summary>
    public class AppSetting
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Key { get; set; } = null!;

        [Required, MaxLength(4096)]
        public string Value { get; set; } = null!;

        public bool IsSecret { get; set; }

        [MaxLength(128)]
        public string? Category { get; set; }

        [MaxLength(512)]
        public string? Description { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UpdatedBy { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public User? UpdatedByUser { get; set; }
    }
}
