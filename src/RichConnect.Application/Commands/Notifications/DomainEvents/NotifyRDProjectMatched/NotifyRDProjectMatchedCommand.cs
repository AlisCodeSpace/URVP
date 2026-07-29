using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectMatched
{
    public class NotifyRDProjectMatchedCommand : IRequest<Unit>
    {
        public Guid RDProjectId { get; set; }
        public Guid SubmittedByUserId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public List<string> MatchedFacultySpecialistNames { get; set; } = new();
        public int TotalMatchesCreated { get; set; }
    }
}
