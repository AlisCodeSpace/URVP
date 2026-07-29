using Microsoft.Extensions.Caching.Memory;
using RICHConnect.Backend.Application.Services.FMIS;
using RICHConnect.Backend.Domain.Enums;
using AUB.APIServices.FMIS.Contracts.Interfaces;
using AUB.APIServices.FMIS.Contracts.Classes;
using ProtoBuf.Grpc;

namespace RICHConnect.Backend.Infrastructure.Services.FMIS;

/// <summary>
/// Special allowed user configuration
/// </summary>
public class SpecialAllowedUser
{
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
}

/// <summary>
/// Implementation of FMIS membership checker with caching
/// </summary>
public class FmisMembershipChecker : IFmisMembershipChecker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FmisMembershipChecker> _logger;
    private readonly IMemoryCache _cache;
    private readonly IFMISService? _fmisService;

    public FmisMembershipChecker(
        IConfiguration configuration,
        ILogger<FmisMembershipChecker> logger,
        IMemoryCache cache
        , IFMISService? fmisService = null
    )
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
        _fmisService = fmisService;
    }

    public async Task<bool> IsEmailAllowedAsync(string email, CancellationToken ct = default)
    {
        var result = await CheckMembershipAsync(email, ct);
        return result.IsAllowed;
    }

    public async Task<FmisMembershipResult> CheckMembershipAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("[FMIS] Empty email provided for membership check");
            return new FmisMembershipResult { IsAllowed = false, Source = "Denied" };
        }

        email = email.ToLowerInvariant().Trim();
        var cacheKey = $"fmis_membership_v2_{email}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out FmisMembershipResult? cachedResult) && cachedResult != null)
        {
            _logger.LogDebug("[FMIS] Cache hit for email: {Email} = {Result}", email, cachedResult.IsAllowed);
            return cachedResult;
        }

        // Check special allowed users list (SECOND priority - after FMIS)
        var specialAllowedUsers = _configuration.GetSection("Fmis:SpecialAllowedUsers").Get<SpecialAllowedUser[]>() ?? Array.Empty<SpecialAllowedUser>();
        var specialUser = specialAllowedUsers.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        
        // FMIS service not registered (startup failed or no endpoint configured).
        // Only allow-listed users may log in; everyone else is denied.
        if (_fmisService == null)
        {
            _logger.LogWarning("[FMIS] FMIS service is not available");

            if (specialUser != null)
            {
                var role = ParseRole(specialUser.Role);
                var result = new FmisMembershipResult { IsAllowed = true, EffectiveRole = role, Source = "SpecialAllowList" };
                CacheResult(cacheKey, result);
                _logger.LogInformation("[FMIS] {Email} allowed via SpecialAllowList (FMIS unavailable)", email);
                return result;
            }

            _logger.LogWarning("[FMIS] {Email} denied — FMIS unavailable and not in allow-list", email);
            var denied = new FmisMembershipResult { IsAllowed = false, Source = "FMISUnavailable" };
            CacheResult(cacheKey, denied);
            return denied;
        }

        try
        {
            var faculties = _configuration.GetSection("Fmis:Faculties").Get<string[]>() ?? Array.Empty<string>();

            // Track whether at least one faculty was successfully queried (cache hit or live call).
            // If every faculty throws, we treat it the same as FMIS being unreachable.
            bool anyFacultySucceeded = false;

            // Check against cached faculty lists to avoid over-fetching during each login
            foreach (var faculty in faculties)
            {
                try
                {
                    var facultyListCacheKey = $"fmis_faculty_list_{faculty}";
                    
                    // Try to get cached faculty list first (5-minute TTL)
                    if (!_cache.TryGetValue(facultyListCacheKey, out FacultyMemberLite[]? cachedMembers))
                    {
                        var request = new GetFacultyMembersRequest { Faculty = faculty };
                        var callContext = new CallContext(new Grpc.Core.CallOptions(cancellationToken: ct));
                        cachedMembers = await _fmisService.GetFacultyMembersInFaculty(request, callContext);
                        
                        // Cache faculty list for 5 minutes to reduce FMIS load during login bursts
                        var facultyCacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        };
                        _cache.Set(facultyListCacheKey, cachedMembers, facultyCacheOptions);
                        _logger.LogDebug("[FMIS] Cached faculty list for {Faculty} with {Count} members", faculty, cachedMembers?.Length ?? 0);
                    }

                    anyFacultySucceeded = true;
                    
                    if (cachedMembers != null && cachedMembers.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogInformation("[FMIS] Email {Email} found in FMIS faculty {Faculty}", email, faculty);
                        var result = new FmisMembershipResult 
                        { 
                            IsAllowed = true, 
                            EffectiveRole = UserRole.FacultySpecialist,
                            Source = "FMIS" 
                        };
                        CacheResult(cacheKey, result);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[FMIS] Error checking faculty {Faculty} for email {Email}", faculty, email);
                }
            }

            // Every configured faculty threw — FMIS is effectively down.
            // Only allow-listed users may log in; everyone else is denied.
            if (faculties.Length > 0 && !anyFacultySucceeded)
            {
                _logger.LogWarning("[FMIS] All {Count} faculty check(s) failed for {Email}; applying strict unavailability policy", faculties.Length, email);

                if (specialUser != null)
                {
                    var role = ParseRole(specialUser.Role);
                    var result = new FmisMembershipResult { IsAllowed = true, EffectiveRole = role, Source = "SpecialAllowList" };
                    CacheResult(cacheKey, result);
                    _logger.LogInformation("[FMIS] {Email} allowed via SpecialAllowList (all-faculty-fail path)", email);
                    return result;
                }

                _logger.LogWarning("[FMIS] {Email} denied — all faculty checks failed and not in allow-list", email);
                var denied = new FmisMembershipResult { IsAllowed = false, Source = "FMISError" };
                CacheResult(cacheKey, denied);
                return denied;
            }

            // Not found in FMIS, check special allowed list (SECOND)
            if (specialUser != null)
            {
                var role = ParseRole(specialUser.Role);
                var result = new FmisMembershipResult 
                { 
                    IsAllowed = true, 
                    EffectiveRole = role,
                    Source = "SpecialAllowList" 
                };
                CacheResult(cacheKey, result);
                _logger.LogInformation("[FMIS] Email {Email} not in FMIS but allowed via SpecialAllowList with role {Role}", email, role);
                return result;
            }
            _logger.LogWarning("[FMIS] Email {Email} not found in FMIS or special allowed list - DENIED", email);
            var deniedResult = new FmisMembershipResult { IsAllowed = false, Source = "Denied" };
            CacheResult(cacheKey, deniedResult);
            return deniedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FMIS] Unexpected error checking FMIS membership for email: {Email}", email);

            if (specialUser != null)
            {
                var role = ParseRole(specialUser.Role);
                var result = new FmisMembershipResult { IsAllowed = true, EffectiveRole = role, Source = "SpecialAllowList" };
                CacheResult(cacheKey, result);
                return result;
            }

            _logger.LogWarning("[FMIS] {Email} denied — unexpected FMIS error and not in allow-list", email);
            var denied = new FmisMembershipResult { IsAllowed = false, Source = "FMISError" };
            CacheResult(cacheKey, denied);
            return denied;
        }
    }

    private UserRole ParseRole(string? roleString)
    {
        if (string.IsNullOrWhiteSpace(roleString))
        {
            return UserRole.FacultySpecialist; // Default
        }

        // Try to parse as enum
        if (Enum.TryParse<UserRole>(roleString, true, out var role))
        {
            return role;
        }

        // Handle common variations
        return roleString.ToLowerInvariant() switch
        {
            "admin" => UserRole.Admin,
            "communitypartner" or "community partner" or "partner" => UserRole.CommunityPartner,
            "richteam" or "rich team" => UserRole.RichTeam,
            "facultyspecialist" or "faculty specialist" or "faculty" => UserRole.FacultySpecialist,
            _ => UserRole.FacultySpecialist // Default fallback
        };
    }

    private void CacheResult(string key, FmisMembershipResult result)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // Cache for 15 minutes
        };
        _cache.Set(key, result, cacheOptions);
    }
}

