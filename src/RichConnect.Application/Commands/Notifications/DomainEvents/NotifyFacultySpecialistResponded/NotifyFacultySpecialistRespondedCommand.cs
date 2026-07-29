using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistResponded
{
    public class NotifyFacultySpecialistRespondedCommand : IRequest<Unit>
    {
        public Guid InviteId { get; set; }
        public Guid ChallengeId { get; set; }
        public Guid FacultySpecialistUserId { get; set; }
    public string FacultySpecialistName { get; set; } = string.Empty;
        public string ResponseText { get; set; } = string.Empty;
    }
}

