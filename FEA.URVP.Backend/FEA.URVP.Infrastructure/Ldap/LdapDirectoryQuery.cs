using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Infrastructure.Ldap;

/// <summary>
/// LDAP filters and lookup order for AUB AD role resolution.
/// Kept free of network I/O so the query shape can be unit tested.
/// </summary>
public static class LdapDirectoryQuery
{
    /// <summary>Active Directory matching rule that walks nested group membership.</summary>
    public const string NestedMemberOid = "1.2.840.113556.1.4.1941";

    public static IReadOnlyList<(string Attribute, string Value)> BuildUserLookupSequence(
        string samAccountName,
        string upn,
        string mail,
        IEnumerable<string> extraAddresses)
    {
        var sequence = new List<(string Attribute, string Value)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Add(sequence, seen, "sAMAccountName", samAccountName);
        Add(sequence, seen, "userPrincipalName", upn);
        Add(sequence, seen, "mail", mail);

        foreach (var address in extraAddresses)
        {
            Add(sequence, seen, "userPrincipalName", address);
            Add(sequence, seen, "mail", address);
            Add(sequence, seen, "proxyAddresses", $"SMTP:{address}");
            Add(sequence, seen, "proxyAddresses", $"smtp:{address}");
        }

        return sequence;
    }

    public static string UserObjectFilter(string attribute, string value)
        => $"(&(objectCategory=person)(objectClass=user)({attribute}={EscapeFilter(value)}))";

    public static string GroupByCnFilter(string cn)
        => $"(&(objectCategory=group)(cn={EscapeFilter(cn)}))";

    public static string NestedMemberFilter(string userDn)
        => $"(member:{NestedMemberOid}:={EscapeFilter(userDn)})";

    public static UserRole? RoleFromMembership(bool inFacultyGroup, bool inStudentGroup)
    {
        if (inFacultyGroup)
        {
            return UserRole.Faculty;
        }

        if (inStudentGroup)
        {
            return UserRole.Student;
        }

        return null;
    }

    public static UserRole? RoleFromGroupCns(
        IEnumerable<string> groupCns,
        string facultyGroup,
        string studentGroup)
    {
        var set = groupCns as ICollection<string> ?? groupCns.ToList();
        return RoleFromMembership(
            set.Contains(facultyGroup, StringComparer.OrdinalIgnoreCase),
            set.Contains(studentGroup, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// "CN=ALLACADstaff-STF,OU=...,DC=aub,DC=edu,DC=lb" → "ALLACADstaff-STF"
    /// </summary>
    public static string? ExtractCnFromDn(string dn)
    {
        if (!dn.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var commaIndex = dn.IndexOf(',');
        return commaIndex > 3 ? dn[3..commaIndex] : dn[3..];
    }

    public static string EscapeFilter(string value)
        => value
            .Replace("\\", "\\5c", StringComparison.Ordinal)
            .Replace("*", "\\2a", StringComparison.Ordinal)
            .Replace("(", "\\28", StringComparison.Ordinal)
            .Replace(")", "\\29", StringComparison.Ordinal)
            .Replace("\0", "\\00", StringComparison.Ordinal);

    private static void Add(
        List<(string Attribute, string Value)> sequence,
        HashSet<string> seen,
        string attribute,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var trimmed = value.Trim();
        if (!seen.Add($"{attribute}={trimmed}"))
        {
            return;
        }

        sequence.Add((attribute, trimmed));
    }
}
