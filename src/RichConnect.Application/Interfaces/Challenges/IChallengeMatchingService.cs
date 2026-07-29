using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Application.DTOs.Faculty;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.Challenges
{
    /// <summary>
    /// Service for challenge matching operations
    /// </summary>
    public interface IChallengeMatchingService
    {
        // Admin Operations
        Task<List<MatchInviteDto>> InviteFacultySpecialistsAsync(Guid challengeId, List<Guid> FacultySpecialistIds);
        Task<MatchFinalizeDto> FinalizeMatchingAsync(Guid challengeId, Guid adminId);
        Task<List<MatchInviteDto>> GetInvitesForChallengeAsync(Guid challengeId);
        
        // Faculty Specialist Operations
        Task<MatchResponseDto> RespondToInviteAsync(Guid inviteId, Guid facultySpecialistId, InviteStatus decision);
        Task<List<FacultySpecialistChallengeDto>> GetFacultySpecialistInvitesAsync(Guid facultySpecialistId);
        Task<List<FacultySpecialistChallengeDto>> GetFacultySpecialistParticipatingAsync(Guid facultySpecialistId);
        
        // Validation
        Task<bool> ValidateInviteExistsAsync(Guid inviteId);
        Task<bool> ValidateInviteBelongsToFacultySpecialistAsync(Guid inviteId, Guid facultySpecialistId);
        Task<bool> ValidateChallengeReadyForFinalizationAsync(Guid challengeId);
    }
}
