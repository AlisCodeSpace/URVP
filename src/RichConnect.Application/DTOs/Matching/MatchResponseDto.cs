using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Matching
{
    /// <summary>
    /// DTO for responding to a match invite
    /// </summary>
    public class MatchResponseDto
    {
        public Guid InviteId { get; set; }
        public InviteStatus Status { get; set; }
        public ChallengeMatchingStatus ChallengeMatchingStatus { get; set; }
        public DateTime RespondedAt { get; set; }
    }
}
