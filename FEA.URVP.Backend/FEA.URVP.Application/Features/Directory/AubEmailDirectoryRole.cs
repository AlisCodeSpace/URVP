using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Directory;

/// <summary>
/// Last-resort Student vs Faculty mapping from the Azure AD mailbox domain
/// when on-prem group membership cannot be resolved.
/// </summary>
/// <remarks>
/// Student domains are tested first so <c>mail.aub.edu.lb</c> is not treated as staff.
/// </remarks>
public static class AubEmailDirectoryRole
{
    public static UserRole? FromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var at = email.LastIndexOf('@');
        if (at < 0 || at == email.Length - 1)
        {
            return null;
        }

        var domain = email[(at + 1)..].Trim();
        if (domain.Equals("mail.aub.edu", StringComparison.OrdinalIgnoreCase)
            || domain.Equals("mail.aub.edu.lb", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Student;
        }

        if (domain.Equals("aub.edu.lb", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Faculty;
        }

        return null;
    }
}
