using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges
{
    /// <summary>
    /// Repository implementation for challenge data access operations
    /// </summary>
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly AppDbContext _context;

        public ChallengeRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Core CRUD Operations

        public async Task<Challenge?> GetByIdAsync(Guid id)
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField) // Include research field (including pending ones)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetByIdWithIncludesAsync(Guid id)
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField)
                .Include(c => c.MatchedFacultySpecialists!)
                    .ThenInclude(mp => mp.FacultySpecialist)
                .Include(c => c.UserSubmitted)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Challenge?> GetChallengeWithUserAsync(Guid challengeId)
        {
            return await _context.Challenges
                .Include(c => c.ResearchField)
                .Include(c => c.UserSubmitted)
                .FirstOrDefaultAsync(c => c.Id == challengeId);
        }

        public async Task<List<Challenge>> GetByStatusAsync(ChallengeStatus status)
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField) // Include research field (including pending ones)
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task<List<Challenge>> GetByStatusWithIncludesAsync(ChallengeStatus status)
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField)
                .Include(c => c.UserSubmitted)
                .Include(c => c.UserApproved)
                .Include(c => c.MatchedFacultySpecialists!)
                    .ThenInclude(mp => mp.FacultySpecialist)
                .Where(c => c.Status == status)
                .ToListAsync();
        }
        
        // Special method to get approved challenges including those with completed matching
        public async Task<List<Challenge>> GetApprovedChallengesForMatchingAsync()
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField)
                .Include(c => c.UserSubmitted)
                .Include(c => c.UserApproved)
                .Include(c => c.MatchedFacultySpecialists!)
                    .ThenInclude(mp => mp.FacultySpecialist)
                .Where(c => c.Status == ChallengeStatus.Approved)
                .ToListAsync();
        }

        public async Task<List<Challenge>> GetByUserAsync(Guid userId)
        {
            return await _context.Challenges
                .AsNoTracking()
                .Include(c => c.ResearchField) // Include research field (including pending ones)
                .Where(c => c.SubmittedBy == userId)
                .ToListAsync();
        }

        public async Task<Challenge> CreateAsync(Challenge challenge)
        {
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();
            return challenge;
        }

        public async Task<Challenge> UpdateAsync(Challenge challenge)
        {
            // Check if an entity with the same key is already being tracked
            var existingEntry = _context.ChangeTracker.Entries<Challenge>()
                .FirstOrDefault(e => e.Entity.Id == challenge.Id);
            
            if (existingEntry != null)
            {
                // Detach the existing tracked entity to avoid conflicts
                existingEntry.State = EntityState.Detached;
            }
            
            _context.Challenges.Update(challenge);
            await _context.SaveChangesAsync();
            return challenge;
        }

        public async Task DeleteAsync(Guid id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge != null)
            {
                _context.Challenges.Remove(challenge);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Challenges.AsNoTracking().AnyAsync(c => c.Id == id);
        }

        #endregion

        #region Matching Operations

        public async Task<List<ChallengeMatchInvite>> GetInvitesByChallengeAsync(Guid challengeId)
        {
            return await _context.ChallengeMatchInvites
                .AsNoTracking()
                .Where(i => i.ChallengeId == challengeId)
                .ToListAsync();
        }

        public async Task<List<ChallengeMatchInvite>> GetInvitesByFacultySpecialistAsync(Guid facultySpecialistId)
        {
            return await _context.ChallengeMatchInvites
                .AsNoTracking()
                .Where(i => i.FacultySpecialistUserId == facultySpecialistId)
                .Include(i => i.Challenge)
                .ThenInclude(c => c.ResearchField)
                .Include(i => i.Challenge.UserSubmitted)
                .ToListAsync();
        }

        public async Task<ChallengeMatchInvite?> GetInviteByIdAsync(Guid inviteId)
        {
            return await _context.ChallengeMatchInvites
                .Include(i => i.Challenge)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
        }

        public async Task<ChallengeMatchInvite> CreateInviteAsync(ChallengeMatchInvite invite)
        {
            _context.ChallengeMatchInvites.Add(invite);
            await _context.SaveChangesAsync();
            return invite;
        }

        public async Task<ChallengeMatchInvite> UpdateInviteAsync(ChallengeMatchInvite invite)
        {
            _context.ChallengeMatchInvites.Update(invite);
            await _context.SaveChangesAsync();
            return invite;
        }

        #endregion

        #region Matched Faculty Specialists Operations

        public async Task<List<ChallengeMatchedFacultySpecialist>> GetMatchedFacultySpecialistsAsync(Guid challengeId)
        {
            return await _context.ChallengeMatchedFacultySpecialists
                .AsNoTracking()
                .Where(mp => mp.ChallengeId == challengeId)
                .ToListAsync();
        }

        public async Task<List<ChallengeMatchedFacultySpecialist>> GetMatchedFacultySpecialistsByFacultySpecialistAsync(Guid facultySpecialistId)
        {
            return await _context.ChallengeMatchedFacultySpecialists
                .AsNoTracking()
                .Where(mp => mp.FacultySpecialistUserId == facultySpecialistId)
                .ToListAsync();
        }

        public async Task<ChallengeMatchedFacultySpecialist> AddMatchedFacultySpecialistAsync(ChallengeMatchedFacultySpecialist match)
        {
            _context.ChallengeMatchedFacultySpecialists.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }

        public async Task RemoveMatchedFacultySpecialistAsync(Guid challengeId, Guid facultySpecialistId)
        {
            var match = await _context.ChallengeMatchedFacultySpecialists
                .FirstOrDefaultAsync(mp => mp.ChallengeId == challengeId && mp.FacultySpecialistUserId == facultySpecialistId);
            
            if (match != null)
            {
                _context.ChallengeMatchedFacultySpecialists.Remove(match);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearMatchedFacultySpecialistsAsync(Guid challengeId)
        {
            var matches = await _context.ChallengeMatchedFacultySpecialists
                .Where(mp => mp.ChallengeId == challengeId)
                .ToListAsync();
            
            _context.ChallengeMatchedFacultySpecialists.RemoveRange(matches);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId)
        {
            return await _context.ResearchFields.AnyAsync(rf => rf.Id == researchFieldId && rf.Status == ApprovalStatus.Approved);
        }

        public async Task<bool> ValidateFacultySpecialistExistsAsync(Guid facultySpecialistId)
        {
            return await _context.Users.AnyAsync(u => u.Id == facultySpecialistId && u.Role == UserRole.FacultySpecialist);
        }

        #endregion
    }
}
