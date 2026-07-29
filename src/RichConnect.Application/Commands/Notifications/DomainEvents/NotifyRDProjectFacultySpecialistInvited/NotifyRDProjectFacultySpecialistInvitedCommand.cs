using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistInvited
{
    public class NotifyRDProjectFacultySpecialistInvitedCommand : IRequest<Unit>
    {
        public Guid InviteId { get; set; }
        public Guid RDProjectId { get; set; }
        public Guid FacultySpecialistUserId { get; set; }
        public string FacultySpecialistName { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string? ProjectDescription { get; set; }
    }
}
