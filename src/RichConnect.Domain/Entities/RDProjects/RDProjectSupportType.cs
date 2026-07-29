using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RICHConnect.Backend.Domain.Entities.RDProjects
{
    public class RDProjectSupportType
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RDProjectId { get; set; }

        [ForeignKey(nameof(RDProjectId))]
        public RDProject RDProject { get; set; } = null!;

        [Required, MaxLength(100)]
        public string SupportTypeValue { get; set; } = null!;
    }
}
