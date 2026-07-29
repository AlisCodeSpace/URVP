using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges
{
    /// <summary>
    /// Repository implementation for ChallengeEditRequest data access operations
    /// </summary>
    public class ChallengeEditRequestRepository : IChallengeEditRequestRepository
    {
        private readonly AppDbContext _context;

        public ChallengeEditRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Core CRUD Operations

        public async Task<ChallengeEditRequest> CreateAsync(ChallengeEditRequest request)
        {
            request.Id = Guid.NewGuid();
            request.RequestedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            request.Status = EditRequestStatus.Pending;

            _context.ChallengeEditRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ChallengeEditRequest?> GetByIdAsync(Guid id)
        {
            return await _context.ChallengeEditRequests
                .AsNoTracking()
                .Include(cer => cer.Challenge)
                .Include(cer => cer.RequestedByUser)
                .Include(cer => cer.RespondedByUser)
                .FirstOrDefaultAsync(cer => cer.Id == id);
        }

        public async Task<List<ChallengeEditRequest>> GetByChallengeIdAsync(Guid challengeId)
        {
            return await _context.ChallengeEditRequests
                .AsNoTracking()
                .Include(cer => cer.RequestedByUser)
                .Include(cer => cer.RespondedByUser)
                .Where(cer => cer.ChallengeId == challengeId)
                .OrderByDescending(cer => cer.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<ChallengeEditRequest>> GetPendingRequestsAsync()
        {
            return await _context.ChallengeEditRequests
                .AsNoTracking()
                .Include(cer => cer.Challenge)
                .Include(cer => cer.RequestedByUser)
                .Where(cer => cer.Status == EditRequestStatus.Pending)
                .OrderBy(cer => cer.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<ChallengeEditRequest>> GetByStatusAsync(EditRequestStatus status)
        {
            return await _context.ChallengeEditRequests
                .AsNoTracking()
                .Include(cer => cer.Challenge)
                .Include(cer => cer.RequestedByUser)
                .Include(cer => cer.RespondedByUser)
                .Where(cer => cer.Status == status)
                .OrderByDescending(cer => cer.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<ChallengeEditRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ChallengeEditRequests
                .AsNoTracking()
                .Include(cer => cer.Challenge)
                .Include(cer => cer.RespondedByUser)
                .Where(cer => cer.RequestedBy == userId)
                .OrderByDescending(cer => cer.RequestedAt)
                .ToListAsync();
        }

        public async Task<ChallengeEditRequest> UpdateAsync(ChallengeEditRequest request)
        {
            // Check if an entity with the same key is already being tracked
            var existingEntry = _context.ChangeTracker.Entries<ChallengeEditRequest>()
                .FirstOrDefault(e => e.Entity.Id == request.Id);
            
            if (existingEntry != null)
            {
                // Detach the existing tracked entity to avoid conflicts
                existingEntry.State = EntityState.Detached;
            }
            
            request.UpdatedAt = DateTime.UtcNow;
            _context.ChallengeEditRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var request = await _context.ChallengeEditRequests.FindAsync(id);
            if (request == null)
                return false;

            _context.ChallengeEditRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasPendingRequestsAsync(Guid challengeId)
        {
            return await _context.ChallengeEditRequests
                .AnyAsync(cer => cer.ChallengeId == challengeId && cer.Status == EditRequestStatus.Pending);
        }

        #endregion
    }
}
