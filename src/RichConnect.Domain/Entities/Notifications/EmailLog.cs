using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Domain.Entities.Notifications
{
    public class EmailLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string From { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string To { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Cc { get; set; }

        [MaxLength(256)]
        public string? Bcc { get; set; }

        [MaxLength(4000)]
        public string? Body { get; set; }

        [MaxLength(4000)]
        public string? Exception { get; set; }

        public bool Success { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
    }
}

