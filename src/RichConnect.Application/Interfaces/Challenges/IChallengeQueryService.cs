using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces
{
    public interface IChallengeQueryService
    {
        Task<ChallengeDto?> GetByIdAsync(Guid id, Guid userId, string userRole);
        Task<List<ChallengeDto>> GetByStatusAsync(ChallengeStatus status);
        Task<List<ChallengeWithDetailsDto>> GetByStatusWithDetailsAsync(ChallengeStatus status);
        Task<List<ChallengeDto>> GetUserChallengesAsync(Guid userId);
    }
}
