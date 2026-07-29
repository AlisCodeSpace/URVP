using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.Admin
{
    public class AdminActionLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AdminUserId { get; set; }

        [ForeignKey(nameof(AdminUserId))]
        public User AdminUser { get; set; } = null!;

        [Required, MaxLength(50)]
        public string ActionType { get; set; } = null!;

        [Required, MaxLength(50)]
        public string EntityType { get; set; } = null!;

        [Required]
        public Guid EntityId { get; set; }

        [MaxLength(64)]
        public string? ClientIpHash { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
