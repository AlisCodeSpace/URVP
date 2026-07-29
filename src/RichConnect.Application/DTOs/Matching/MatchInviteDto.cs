using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Matching
{
    public class MatchInviteDto
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public Guid FacultySpecialistUserId { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


