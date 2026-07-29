using MediatR;
using RICHConnect.Backend.Application.DTOs.Matching;

namespace RICHConnect.Backend.Application.Commands.InviteFacultySpecialists
{
    public class InviteFacultySpecialistsCommand : IRequest<List<MatchInviteDto>>
    {
        public Guid ChallengeId { get; set; }
        public List<Guid> FacultySpecialistIds { get; set; }
        public Guid AdminId { get; set; }

        public InviteFacultySpecialistsCommand(Guid challengeId, List<Guid> facultySpecialistIds, Guid adminId)
        {
            ChallengeId = challengeId;
            FacultySpecialistIds = facultySpecialistIds;
            AdminId = adminId;
        }
    }
}
