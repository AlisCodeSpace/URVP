using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistResponded
{
    public class NotifyRDProjectFacultySpecialistRespondedCommand : IRequest<Unit>
    {
        public Guid InviteId { get; set; }
        public Guid RDProjectId { get; set; }
        public Guid FacultySpecialistUserId { get; set; }
    public string FacultySpecialistName { get; set; } = string.Empty;
        public string ResponseText { get; set; } = string.Empty;
    }
}
