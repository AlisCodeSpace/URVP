using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Themes
{
    /// <summary>
    /// Repository implementation for ResearchTheme operations
    /// </summary>
    public class ThemeRepository : IThemeRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ThemeRepository> _logger;
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "Theme:";
        private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new()
        {
            SlidingExpiration = TimeSpan.FromHours(1),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        public ThemeRepository(AppDbContext context, ILogger<ThemeRepository> logger, IDistributedCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task InvalidateCacheAsync()
        {
            // Invalidate all theme caches by removing common keys
            var keysToRemove = new[]
            {
                $"{CacheKeyPrefix}All",
                $"{CacheKeyPrefix}Approved",
                $"{CacheKeyPrefix}Status:{ApprovalStatus.Approved}"
            };
            
            foreach (var key in keysToRemove)
            {
                await _cache.RemoveAsync(key);
            }
        }
        
        private static string GetCacheKey(string suffix) => $"{CacheKeyPrefix}{suffix}";

        #region Core CRUD Operations

        public async Task<ResearchTheme?> GetByIdAsync(Guid id)
        {
            try
            {
                var cacheKey = GetCacheKey($"Id:{id}");
                var cachedBytes = await _cache.GetAsync(cacheKey);
                
                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                    return JsonSerializer.Deserialize<ResearchTheme>(cachedJson);
                }
                
                var theme = await _context.Themes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
                
                if (theme != null)
                {
                    var json = JsonSerializer.Serialize(theme);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
                }
                
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving theme by ID: {ThemeId}", id);
                throw;
            }
        }

        public async Task<ResearchTheme?> GetByIdWithIncludesAsync(Guid id)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving theme by ID with includes: {ThemeId}", id);
                throw;
            }
        }

        public async Task<ResearchTheme?> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            try
            {
                var cacheKey = GetCacheKey($"Slug:{slug}");
                var cachedBytes = await _cache.GetAsync(cacheKey);
                
                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                    return JsonSerializer.Deserialize<ResearchTheme>(cachedJson);
                }
                
                var theme = await _context.Themes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Slug == slug);
                
                if (theme != null)
                {
                    var json = JsonSerializer.Serialize(theme);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
                }
                
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving theme by slug: {Slug}", slug);
                throw;
            }
        }

        public async Task<ResearchTheme?> GetBySlugWithIncludesAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .FirstOrDefaultAsync(t => t.Slug == slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving theme by slug with includes: {Slug}", slug);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetAllAsync()
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all themes");
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetAllWithIncludesAsync()
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all themes with includes");
                throw;
            }
        }

        public async Task<ResearchTheme> CreateAsync(ResearchTheme theme)
        {
            try
            {
                _context.Themes.Add(theme);
                await _context.SaveChangesAsync();
                
                // Invalidate cache after creating
                await InvalidateCacheAsync();
                
                _logger.LogInformation("Created theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating theme: {Title}", theme.Title);
                throw;
            }
        }

        public async Task<ResearchTheme> UpdateAsync(ResearchTheme theme)
        {
            try
            {
                // Check if an entity with the same key is already being tracked
                var existingEntry = _context.ChangeTracker.Entries<ResearchTheme>()
                    .FirstOrDefault(e => e.Entity.Id == theme.Id);
                
                if (existingEntry != null)
                {
                    // Detach the existing tracked entity to avoid conflicts
                    existingEntry.State = EntityState.Detached;
                }
                
                _context.Themes.Update(theme);
                await _context.SaveChangesAsync();
                
                // Invalidate cache for this specific theme and list caches
                await _cache.RemoveAsync(GetCacheKey($"Id:{theme.Id}"));
                if (!string.IsNullOrEmpty(theme.Slug))
                {
                    await _cache.RemoveAsync(GetCacheKey($"Slug:{theme.Slug}"));
                }
                await InvalidateCacheAsync();
                
                _logger.LogInformation("Updated theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var theme = await _context.Themes.FindAsync(id);
                if (theme != null)
                {
                    _context.Themes.Remove(theme);
                    await _context.SaveChangesAsync();
                    
                    // Invalidate cache for this specific theme and list caches
                    await _cache.RemoveAsync(GetCacheKey($"Id:{id}"));
                    if (!string.IsNullOrEmpty(theme.Slug))
                    {
                        await _cache.RemoveAsync(GetCacheKey($"Slug:{theme.Slug}"));
                    }
                    await InvalidateCacheAsync();
                    
                    _logger.LogInformation("Deleted theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting theme: {ThemeId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.Themes.AsNoTracking().AnyAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking theme existence: {ThemeId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            try
            {
                return await _context.Themes.AsNoTracking().AnyAsync(t => t.Slug == slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking theme slug existence: {Slug}", slug);
                throw;
            }
        }

        #endregion

        #region Status-based queries

        public async Task<List<ResearchTheme>> GetByStatusAsync(ApprovalStatus status)
        {
            try
            {
                // Cache only approved themes as they're frequently accessed
                if (status == ApprovalStatus.Approved)
                {
                    var cacheKey = GetCacheKey($"Status:{status}");
                    var cachedBytes = await _cache.GetAsync(cacheKey);
                    
                    if (cachedBytes != null && cachedBytes.Length > 0)
                    {
                        var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                        return JsonSerializer.Deserialize<List<ResearchTheme>>(cachedJson) ?? new List<ResearchTheme>();
                    }
                    
                    var themes = await _context.Themes
                        .AsNoTracking()
                        .Where(t => t.Status == status)
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync();
                    
                    if (themes.Any())
                    {
                        var json = JsonSerializer.Serialize(themes);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _cache.SetAsync(cacheKey, bytes, DefaultCacheOptions);
                    }
                    
                    return themes;
                }
                
                // For non-approved statuses, don't cache (they change frequently)
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.Status == status)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by status: {Status}", status);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByStatusWithIncludesAsync(ApprovalStatus status)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.Status == status)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by status with includes: {Status}", status);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetPendingAsync()
        {
            return await GetByStatusAsync(ApprovalStatus.Pending);
        }

        public async Task<List<ResearchTheme>> GetPendingWithIncludesAsync()
        {
            return await GetByStatusWithIncludesAsync(ApprovalStatus.Pending);
        }

        public async Task<List<ResearchTheme>> GetApprovedAsync()
        {
            return await GetByStatusAsync(ApprovalStatus.Approved);
        }

        public async Task<List<ResearchTheme>> GetApprovedWithIncludesAsync()
        {
            return await GetByStatusWithIncludesAsync(ApprovalStatus.Approved);
        }

        public async Task<List<ResearchTheme>> GetRejectedAsync()
        {
            return await GetByStatusAsync(ApprovalStatus.Rejected);
        }

        public async Task<List<ResearchTheme>> GetRejectedWithIncludesAsync()
        {
            return await GetByStatusWithIncludesAsync(ApprovalStatus.Rejected);
        }

        #endregion

        #region User-based queries

        public async Task<List<ResearchTheme>> GetByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.SubmittedBy == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByUserWithIncludesAsync(Guid userId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.SubmittedBy == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by user with includes: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByApproverAsync(Guid approverId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.ApprovedBy == approverId)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by approver: {ApproverId}", approverId);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByApproverWithIncludesAsync(Guid approverId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.ApprovedBy == approverId)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by approver with includes: {ApproverId}", approverId);
                throw;
            }
        }

        #endregion

        #region Research field queries

        public async Task<List<ResearchTheme>> GetByResearchFieldAsync(Guid researchFieldId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.ResearchFieldId == researchFieldId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by research field: {ResearchFieldId}", researchFieldId);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByResearchFieldWithIncludesAsync(Guid researchFieldId)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.ResearchFieldId == researchFieldId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by research field with includes: {ResearchFieldId}", researchFieldId);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByResearchFieldAndStatusAsync(Guid researchFieldId, ApprovalStatus status)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.ResearchFieldId == researchFieldId && t.Status == status)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by research field and status: {ResearchFieldId}, {Status}", researchFieldId, status);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByResearchFieldAndStatusWithIncludesAsync(Guid researchFieldId, ApprovalStatus status)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.ResearchFieldId == researchFieldId && t.Status == status)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by research field and status with includes: {ResearchFieldId}, {Status}", researchFieldId, status);
                throw;
            }
        }

        #endregion

        #region Admin queries

        public async Task<List<ResearchTheme>> GetForAdminReviewAsync()
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.Status == ApprovalStatus.Pending)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes for admin review");
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetForAdminReviewWithIncludesAsync()
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Where(t => t.Status == ApprovalStatus.Pending)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes for admin review with includes");
                throw;
            }
        }

        public async Task<int> GetCountByStatusAsync(ApprovalStatus status)
        {
            try
            {
                return await _context.Themes.CountAsync(t => t.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count by status: {Status}", status);
                throw;
            }
        }

        public async Task<int> GetCountByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Themes.CountAsync(t => t.SubmittedBy == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count by user: {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetCountByResearchFieldAsync(Guid researchFieldId)
        {
            try
            {
                return await _context.Themes.CountAsync(t => t.ResearchFieldId == researchFieldId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count by research field: {ResearchFieldId}", researchFieldId);
                throw;
            }
        }

        #endregion

        #region Search and filtering

        public async Task<List<ResearchTheme>> SearchByTitleAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ResearchTheme>();

            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.Title.Contains(searchTerm))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching themes by title: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> SearchByTitleWithIncludesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ResearchTheme>();

            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.Title.Contains(searchTerm))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching themes by title with includes: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by date range: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByDateRangeWithIncludesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by date range with includes: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        #endregion

        #region Validation methods

        public async Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId)
        {
            try
            {
                return await _context.ResearchFields.AnyAsync(rf => rf.Id == researchFieldId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating research field exists: {ResearchFieldId}", researchFieldId);
                throw;
            }
        }

        public async Task<bool> ValidateUserExistsAsync(Guid userId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user exists: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateSlugIsUniqueAsync(string slug, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            try
            {
                var query = _context.Themes.AsNoTracking().Where(t => t.Slug == slug);
                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }
                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating slug uniqueness: {Slug}", slug);
                throw;
            }
        }

        public async Task<bool> ValidateTitleIsUniqueAsync(string title, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            try
            {
                var normalizedTitle = title.Trim().ToLowerInvariant();
                var query = _context.Themes.AsNoTracking().Where(t => t.Title.ToLower() == normalizedTitle);
                if (excludeId.HasValue)
                {
                    query = query.Where(t => t.Id != excludeId.Value);
                }
                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating title uniqueness: {Title}", title);
                throw;
            }
        }

        public async Task<bool> HasDependenciesAsync(Guid themeId)
        {
            try
            {
                // Check if theme exists
                var theme = await _context.Themes.FindAsync(themeId);
                if (theme == null)
                {
                    return false;
                }

                // Note: Challenges and RDProjects no longer have a direct ResearchThemeId relationship
                // (it was removed in migration 20251023111043_RemoveResearchThemeIdFromChallengesV2)
                // Therefore, there are no direct dependencies between themes and challenges/R&D projects.
                // Themes can be deleted without checking for challenges or R&D projects.
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking dependencies for theme: {ThemeId}", themeId);
                throw;
            }
        }

        #endregion

        #region Bulk operations

        public async Task<List<ResearchTheme>> GetByIdsAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return new List<ResearchTheme>();

            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => ids.Contains(t.Id))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by IDs: {Ids}", string.Join(", ", ids));
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetByIdsWithIncludesAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return new List<ResearchTheme>();

            try
            {
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => ids.Contains(t.Id))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving themes by IDs with includes: {Ids}", string.Join(", ", ids));
                throw;
            }
        }

        public async Task<int> DeleteByStatusAsync(ApprovalStatus status)
        {
            try
            {
                var themes = await _context.Themes.Where(t => t.Status == status).ToListAsync();
                _context.Themes.RemoveRange(themes);
                var count = await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} themes with status: {Status}", count, status);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting themes by status: {Status}", status);
                throw;
            }
        }

        public async Task<int> UpdateStatusAsync(List<Guid> ids, ApprovalStatus status, Guid updatedBy)
        {
            if (ids == null || !ids.Any())
                return 0;

            try
            {
                var themes = await _context.Themes.Where(t => ids.Contains(t.Id)).ToListAsync();
                foreach (var theme in themes)
                {
                    theme.Status = status;
                    theme.ApprovedBy = updatedBy;
                    theme.UpdatedAt = DateTime.UtcNow;
                }
                var count = await _context.SaveChangesAsync();
                
                // Invalidate cache when status changes (especially if changing to/from Approved)
                foreach (var theme in themes)
                {
                    await _cache.RemoveAsync(GetCacheKey($"Id:{theme.Id}"));
                    if (!string.IsNullOrEmpty(theme.Slug))
                    {
                        await _cache.RemoveAsync(GetCacheKey($"Slug:{theme.Slug}"));
                    }
                }
                await InvalidateCacheAsync();
                
                _logger.LogInformation("Updated status for {Count} themes to: {Status}", count, status);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for themes: {Ids} to {Status}", string.Join(", ", ids), status);
                throw;
            }
        }

        #endregion

        #region Statistics and analytics

        public async Task<Dictionary<ApprovalStatus, int>> GetStatusCountsAsync()
        {
            try
            {
                return await _context.Themes
                    .GroupBy(t => t.Status)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status counts");
                throw;
            }
        }

        public async Task<Dictionary<Guid, int>> GetCountsByResearchFieldAsync()
        {
            try
            {
                return await _context.Themes
                    .Where(t => t.ResearchFieldId.HasValue)
                    .GroupBy(t => t.ResearchFieldId!.Value)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counts by research field");
                throw;
            }
        }

        public async Task<Dictionary<Guid, int>> GetCountsByUserAsync()
        {
            try
            {
                return await _context.Themes
                    .GroupBy(t => t.SubmittedBy)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counts by user");
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetRecentlyUpdatedAsync(int days = 7)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                return await _context.Themes
                    .AsNoTracking()
                    .Where(t => t.UpdatedAt >= cutoffDate)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recently updated themes: {Days} days", days);
                throw;
            }
        }

        public async Task<List<ResearchTheme>> GetRecentlyUpdatedWithIncludesAsync(int days = 7)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                return await _context.Themes
                    .AsNoTracking()
                    .Include(t => t.ResearchField)
                    .Include(t => t.UserSubmitted)
                    .Include(t => t.UserApproved)
                    .Where(t => t.UpdatedAt >= cutoffDate)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recently updated themes with includes: {Days} days", days);
                throw;
            }
        }

        public async Task<ResearchTheme?> GetThemeWithUserAsync(Guid themeId)
        {
            try
            {
                return await _context.Themes
                    .Include(t => t.UserSubmitted)
                    .FirstOrDefaultAsync(t => t.Id == themeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving theme with user by ID: {ThemeId}", themeId);
                throw;
            }
        }

        #endregion
    }
}
