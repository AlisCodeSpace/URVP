using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields
{
    public class ResearchFieldRepository : IResearchFieldRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "ResearchField:";
        private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromHours(1),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public ResearchFieldRepository(AppDbContext context, IDistributedCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        
        private async Task InvalidateCacheAsync()
        {
            // Invalidate all research field caches by removing common keys
            // Note: In a production system, you might want to use cache tags/patterns
            // For now, we'll invalidate on write operations
            var keysToRemove = new[]
            {
                $"{CacheKeyPrefix}AllActive",
                $"{CacheKeyPrefix}AllIncludingInactive"
            };
            
            foreach (var key in keysToRemove)
            {
                await _cache.RemoveAsync(key);
            }
        }
        
        private static string GetCacheKey(string suffix) => $"{CacheKeyPrefix}{suffix}";

        public async Task<ResearchField?> GetByIdAsync(Guid id)
        {
            var cacheKey = GetCacheKey($"Id:{id}");
            var cachedBytes = await _cache.GetAsync(cacheKey);
            
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<ResearchField>(cachedJson);
            }
            
            var field = await _context.ResearchFields
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (field != null)
            {
                var json = JsonSerializer.Serialize(field);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
            }
            
            return field;
        }

        public async Task<ResearchField?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            var cacheKey = GetCacheKey($"Slug:{slug}");
            var cachedBytes = await _cache.GetAsync(cacheKey);
            
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<ResearchField>(cachedJson);
            }
            
            var field = await _context.ResearchFields
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Slug == slug);
            
            if (field != null)
            {
                var json = JsonSerializer.Serialize(field);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
            }
            
            return field;
        }

        public async Task<IEnumerable<ResearchField>> GetAllActiveAsync()
        {
            var cacheKey = GetCacheKey("AllActive");
            var cachedBytes = await _cache.GetAsync(cacheKey);
            
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<List<ResearchField>>(cachedJson) ?? new List<ResearchField>();
            }
            
            var fields = await _context.ResearchFields
                .AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
            
            if (fields.Any())
            {
                var json = JsonSerializer.Serialize(fields);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
            }
            
            return fields;
        }

        public async Task<IEnumerable<ResearchField>> GetAllIncludingInactiveAsync()
        {
            return await _context.ResearchFields
                .AsNoTracking()
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResearchField>> GetByStatusAsync(ApprovalStatus status)
        {
            return await _context.ResearchFields
                .AsNoTracking()
                .Where(f => f.Status == status)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResearchField>> GetBySubmitterAsync(Guid userId)
        {
            return await _context.ResearchFields
                .AsNoTracking()
                .Where(f => f.SubmittedBy == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResearchField>> GetAvailableFieldsForUserAsync(Guid userId)
        {
            return await _context.ResearchFields
                .AsNoTracking()
                .Where(f => f.IsActive && 
                           f.Status == ApprovalStatus.Approved && 
                           f.SubmittedBy != userId)
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<ResearchField> AddAsync(ResearchField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            await _context.ResearchFields.AddAsync(field);
            await _context.SaveChangesAsync();
            
            // Invalidate cache after adding
            await InvalidateCacheAsync();
            
            return field;
        }

        public async Task<ResearchField?> UpdateAsync(ResearchField field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var existingField = await _context.ResearchFields.FindAsync(field.Id);
            if (existingField == null)
                return null;

            // Update entity properties
            _context.Entry(existingField).CurrentValues.SetValues(field);
            existingField.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this specific field and list caches
            await _cache.RemoveAsync(GetCacheKey($"Id:{field.Id}"));
            if (!string.IsNullOrEmpty(field.Slug))
            {
                await _cache.RemoveAsync(GetCacheKey($"Slug:{field.Slug}"));
            }
            await InvalidateCacheAsync();
            
            return existingField;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var field = await _context.ResearchFields.FindAsync(id);
            if (field == null)
                return false;

            _context.ResearchFields.Remove(field);
            await _context.SaveChangesAsync();
            
            // Invalidate cache for this specific field and list caches
            await _cache.RemoveAsync(GetCacheKey($"Id:{id}"));
            if (!string.IsNullOrEmpty(field.Slug))
            {
                await _cache.RemoveAsync(GetCacheKey($"Slug:{field.Slug}"));
            }
            await InvalidateCacheAsync();
            
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ResearchFields.AnyAsync(f => f.Id == id);
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            return await _context.ResearchFields.AnyAsync(f => f.Slug == slug);
        }

        public async Task<ResearchField?> GetFieldWithUserAsync(Guid fieldId)
        {
            return await _context.ResearchFields
                .Include(f => f.UserSubmitted)
                .FirstOrDefaultAsync(f => f.Id == fieldId);
        }
    }
}
