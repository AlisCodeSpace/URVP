using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Matching
{
    /// <summary>
    /// DTO for responding to a challenge invite
    /// </summary>
    public class RespondToInviteDto
    {
        public InviteStatus Decision { get; set; }
    }
}
