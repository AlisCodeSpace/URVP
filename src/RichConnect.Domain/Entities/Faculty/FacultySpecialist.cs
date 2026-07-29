using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Entities.ResearchFields;

namespace RICHConnect.Backend.Domain.Entities.Faculty
{
    public class FacultySpecialist
    {
        [Key]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

    /// <summary>
    /// JSON array of research interest topics
    /// </summary>
    [MaxLength(2000)]
    public string? ResearchInterestsJson { get; set; }

    /// <summary>
    /// Availability status: 0 = Unavailable, 1 = Available
    /// </summary>
    public int Status { get; set; } = 1; // Default to Available

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Helper property to get/set research interests as string array
        /// </summary>
        [NotMapped]
        public string[]? ResearchInterests
        {
            get
            {
                if (string.IsNullOrEmpty(ResearchInterestsJson))
                    return null;
                
                try
                {
                    return JsonSerializer.Deserialize<string[]>(ResearchInterestsJson);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                ResearchInterestsJson = value != null ? JsonSerializer.Serialize(value) : null;
            }
        }

        /// <summary>
        /// Navigation property for research field relationships
        /// </summary>
        public ICollection<FacultySpecialistResearchField>? ResearchFieldLinks { get; set; }
    }
}
