using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces
{
    /// <summary>
    /// Repository for challenge data access operations
    /// </summary>
    public interface IChallengeRepository
    {
        // Core CRUD Operations
        Task<Challenge?> GetByIdAsync(Guid id);
        Task<Challenge?> GetByIdWithIncludesAsync(Guid id);
        Task<Challenge?> GetChallengeWithUserAsync(Guid challengeId);
        Task<List<Challenge>> GetByStatusAsync(ChallengeStatus status);
        Task<List<Challenge>> GetByStatusWithIncludesAsync(ChallengeStatus status);
        
        // Special method to get approved challenges including those with completed matching
        Task<List<Challenge>> GetApprovedChallengesForMatchingAsync();
        Task<List<Challenge>> GetByUserAsync(Guid userId);
        Task<Challenge> CreateAsync(Challenge challenge);
        Task<Challenge> UpdateAsync(Challenge challenge);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        
        // Matching Operations
        Task<List<ChallengeMatchInvite>> GetInvitesByChallengeAsync(Guid challengeId);
        Task<List<ChallengeMatchInvite>> GetInvitesByFacultySpecialistAsync(Guid facultySpecialistId);
        Task<ChallengeMatchInvite?> GetInviteByIdAsync(Guid inviteId);
        Task<ChallengeMatchInvite> CreateInviteAsync(ChallengeMatchInvite invite);
        Task<ChallengeMatchInvite> UpdateInviteAsync(ChallengeMatchInvite invite);
        
        // Matched Faculty Specialists Operations
        Task<List<ChallengeMatchedFacultySpecialist>> GetMatchedFacultySpecialistsAsync(Guid challengeId);
        Task<List<ChallengeMatchedFacultySpecialist>> GetMatchedFacultySpecialistsByFacultySpecialistAsync(Guid facultySpecialistId);
        Task<ChallengeMatchedFacultySpecialist> AddMatchedFacultySpecialistAsync(ChallengeMatchedFacultySpecialist match);
        Task RemoveMatchedFacultySpecialistAsync(Guid challengeId, Guid facultySpecialistId);
        Task ClearMatchedFacultySpecialistsAsync(Guid challengeId);
        
        // Validation
        Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId);
        Task<bool> ValidateFacultySpecialistExistsAsync(Guid facultySpecialistId);
    }
}
