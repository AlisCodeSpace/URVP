using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.RDProjects
{
    public class RDProjectEditRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RDProjectId { get; set; }

        [ForeignKey(nameof(RDProjectId))]
        public RDProject RDProject { get; set; } = null!;

        [Required]
        public Guid RequestedBy { get; set; }

        [ForeignKey(nameof(RequestedBy))]
        public User RequestedByUser { get; set; } = null!;

        [Required, MaxLength(1000)]
        public string EditReason { get; set; } = null!;

        [Required]
        public RDProjectEditRequestStatus Status { get; set; } = RDProjectEditRequestStatus.Pending;

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? RespondedBy { get; set; }

        [ForeignKey(nameof(RespondedBy))]
        public User? RespondedByUser { get; set; }

        [MaxLength(1000)]
        public string? AdminResponse { get; set; }

        public DateTime? RespondedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
