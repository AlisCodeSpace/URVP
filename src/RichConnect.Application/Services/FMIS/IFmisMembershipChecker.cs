using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.FMIS;

/// <summary>
/// Result of FMIS membership check with role information
/// </summary>
public class FmisMembershipResult
{
    public bool IsAllowed { get; set; }
    public UserRole? EffectiveRole { get; set; }
    public string Source { get; set; } = string.Empty; // "FMIS", "SpecialAllowList", or "Denied"
}

/// <summary>
/// Service to check if an email exists in FMIS or is in the exception allow-list
/// </summary>
public interface IFmisMembershipChecker
{
    /// <summary>
    /// Checks if the email is allowed to login via AUB SSO
    /// </summary>
    /// <param name="email">User email to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if email is in FMIS or in exception list, false otherwise</returns>
    Task<bool> IsEmailAllowedAsync(string email, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if the email is allowed to login via AUB SSO and returns role information
    /// </summary>
    /// <param name="email">User email to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Membership result with allow status and effective role</returns>
    Task<FmisMembershipResult> CheckMembershipAsync(string email, CancellationToken ct = default);
}

