using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces
{
    public interface IChallengeApplicationService
    {
        // Commands
        Task<ChallengeDto> CreateChallengeAsync(CreateChallengeDto dto, Guid userId);
        Task<ChallengeDto> ApproveChallengeAsync(Guid id, Guid adminId);
        Task<ChallengeDto> RejectChallengeAsync(Guid id, RejectChallengeDto dto, Guid adminId);
        Task<ChallengeDto> UpdateChallengeAsync(Guid id, UpdateChallengeDto dto, Guid userId);
        Task<ChallengeEditRequestDto> RequestChallengeEditAsync(Guid challengeId, RequestChallengeEditDto dto, Guid userId);
        Task<ChallengeEditRequestDto?> GetEditRequestStatusAsync(Guid challengeId, Guid userId);
        Task<ChallengeEditRequestDto> ApproveEditRequestAsync(Guid editRequestId, ApproveEditRequestDto dto, Guid adminId);
        Task<ChallengeEditRequestDto> RejectEditRequestAsync(Guid editRequestId, RejectEditRequestDto dto, Guid adminId);
        Task<ChallengeEditRequestDto?> GetEditRequestDetailsAsync(Guid editRequestId);
        Task<ChallengeEditRequestDto?> GetEditRequestByChallengeIdAsync(Guid challengeId);
        
        // Matching Commands
        Task<List<MatchInviteDto>> InviteFacultySpecialistsAsync(Guid challengeId, List<Guid> FacultySpecialistIds, Guid adminId);
        Task<MatchFinalizeDto> FinalizeMatchingAsync(Guid challengeId, Guid adminId);
        
        // Queries
        Task<ChallengeDto?> GetChallengeByIdAsync(Guid id, Guid userId, string userRole);
        Task<List<ChallengeDto>> GetChallengesByStatusAsync(ChallengeStatus status);
        Task<List<ChallengeWithDetailsDto>> GetChallengesByStatusWithDetailsAsync(ChallengeStatus status);
        Task<List<ChallengeWithDetailsDto>> GetApprovedChallengesForMatchingAsync();
        Task<List<ChallengeDto>> GetUserChallengesAsync(Guid userId);
        
        // Validation
        Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId);
        Task<bool> ValidateChallengeExistsAsync(Guid challengeId);
        Task<bool> ValidateChallengeStatusAsync(Guid challengeId, ChallengeStatus expectedStatus);
    }
}
