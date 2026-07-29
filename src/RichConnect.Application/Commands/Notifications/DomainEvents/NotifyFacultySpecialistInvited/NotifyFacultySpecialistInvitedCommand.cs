using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistInvited
{
    public class NotifyFacultySpecialistInvitedCommand : IRequest<Unit>
    {
        public Guid InviteId { get; set; }
        public Guid ChallengeId { get; set; }
        public Guid FacultySpecialistUserId { get; set; }
        public string FacultySpecialistName { get; set; } = string.Empty;
        public string ChallengeTitle { get; set; } = string.Empty;
        public string? ThemeName { get; set; }
        public string? PartnerName { get; set; }
        public string? ChallengeDescription { get; set; }
    }
}
