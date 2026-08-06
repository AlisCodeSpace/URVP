using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Catalog;

/// <summary>
/// Fixed accounts for Development-only email sign-in. Never used in production.
/// </summary>
public static class DevAuthAccounts
{
    public const string Affiliation = "URVP Development";

    public sealed record Account(string Email, string Name, string UserName, UserRole Role);

    public static readonly IReadOnlyList<Account> All =
    [
        new("faculty@urvp.com", "Dev Faculty", "faculty", UserRole.Faculty),
        new("student@urvp.com", "Dev Student", "student", UserRole.Student),
        new("admin@urvp.com", "Dev Admin", "admin", UserRole.Admin),
    ];

    private static readonly Dictionary<string, Account> ByEmail = All.ToDictionary(
        a => a.Email,
        StringComparer.OrdinalIgnoreCase);

    public static bool TryGet(string? email, out Account? account)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            account = null;
            return false;
        }

        return ByEmail.TryGetValue(email.Trim(), out account);
    }
}
