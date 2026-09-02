using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Directory;

/// <summary>
/// Looks up a signed-in user's AUB Active Directory group membership
/// (Students-STD / ALLACADstaff-STF) to resolve Student vs Faculty.
/// </summary>
public interface IDirectoryGroupLookup
{
    /// <summary>
    /// Resolves Student or Faculty from AD group membership.
    /// Searches in order: <c>sAMAccountName</c>, <c>userPrincipalName</c>, then <c>mail</c>.
    /// Returns <c>null</c> when AD is disabled, unreachable, the account is missing,
    /// or the user is in neither group.
    /// </summary>
    UserRole? ResolveRole(string preferredUsername, string email);
}
