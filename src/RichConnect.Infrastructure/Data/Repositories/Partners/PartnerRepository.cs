using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using System.Text;
using System.Text.Json;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Partners;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Partners
{
    /// <summary>
    /// Repository implementation for CommunityPartner operations
    /// </summary>
    public class PartnerRepository : IPartnerRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "Partner:";
        private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromHours(1),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public PartnerRepository(AppDbContext context, IDistributedCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task InvalidateCacheAsync()
        {
            // Invalidate all partner caches by removing common keys
            var keysToRemove = new[]
            {
                $"{CacheKeyPrefix}All",
                $"{CacheKeyPrefix}Status:{ApprovalStatus.Approved}",
                $"{CacheKeyPrefix}Pending"
            };
            
            foreach (var key in keysToRemove)
            {
                await _cache.RemoveAsync(key);
            }
        }
        
        private static string GetCacheKey(string suffix) => $"{CacheKeyPrefix}{suffix}";

        /// <inheritdoc />
        public async Task<CommunityPartner?> GetByIdAsync(Guid id)
        {
            var cacheKey = GetCacheKey($"Id:{id}");
            var cachedBytes = await _cache.GetAsync(cacheKey);
            
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<CommunityPartner>(cachedJson);
            }
            
            var partner = await _context.CommunityPartners
                .Include(p => p.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (partner != null)
            {
                var json = JsonSerializer.Serialize(partner);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
            }
            
            return partner;
        }

        /// <inheritdoc />
        public async Task<CommunityPartner?> GetByUserIdAsync(Guid userId)
        {
            return await _context.CommunityPartners
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<List<CommunityPartner>> GetByStatusAsync(ApprovalStatus status)
        {
            // Cache only approved partners as they're frequently accessed
            if (status == ApprovalStatus.Approved)
            {
                var cacheKey = GetCacheKey($"Status:{status}");
                var cachedBytes = await _cache.GetAsync(cacheKey);
                
                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                    return JsonSerializer.Deserialize<List<CommunityPartner>>(cachedJson) ?? new List<CommunityPartner>();
                }
                
                var partners = await _context.CommunityPartners
                    .Include(p => p.User)
                    .AsNoTracking()
                    .Where(p => p.Status == status)
                    .OrderByDescending(p => p.SubmittedAt)
                    .ToListAsync();
                
                if (partners.Any())
                {
                    var json = JsonSerializer.Serialize(partners);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
                }
                
                return partners;
            }
            
            // For non-approved statuses, don't cache (they change frequently)
            return await _context.CommunityPartners
                .Include(p => p.User)
                .AsNoTracking()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<CommunityPartner>> GetAllAsync()
        {
            return await _context.CommunityPartners
                .Include(p => p.User)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ExistsForUserAsync(Guid userId)
        {
            return await _context.CommunityPartners
                .AnyAsync(p => p.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<CommunityPartner> AddAsync(CommunityPartner partner)
        {
            _context.CommunityPartners.Add(partner);
            await _context.SaveChangesAsync();
            
            // Invalidate cache after adding
            await InvalidateCacheAsync();
            
            return partner;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(CommunityPartner partner)
        {
            _context.Entry(partner).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this specific partner and list caches
            await _cache.RemoveAsync(GetCacheKey($"Id:{partner.Id}"));
            await InvalidateCacheAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            var partner = await _context.CommunityPartners.FindAsync(id);
            if (partner != null)
            {
                _context.CommunityPartners.Remove(partner);
                await _context.SaveChangesAsync();
                
                // Invalidate cache for this specific partner and list caches
                await _cache.RemoveAsync(GetCacheKey($"Id:{id}"));
                await InvalidateCacheAsync();
            }
        }

        /// <inheritdoc />
        public async Task<List<CommunityPartner>> GetPendingPartnersAsync()
        {
            return await _context.CommunityPartners
                .Include(p => p.User)
                .Where(p => p.Status == ApprovalStatus.Pending)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetCountByStatusAsync(ApprovalStatus status)
        {
            return await _context.CommunityPartners
                .CountAsync(p => p.Status == status);
        }

        /// <inheritdoc />
        public async Task<CommunityPartner?> GetPartnerWithUserAsync(Guid partnerId)
        {
            return await _context.CommunityPartners
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == partnerId);
        }
    }
}