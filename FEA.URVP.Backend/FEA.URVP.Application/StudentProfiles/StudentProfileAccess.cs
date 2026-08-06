using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.StudentProfiles;

internal static class StudentProfileAccess
{
    /// <summary>Temporary FE testing override — remove when Student role assignment exists.</summary>
    private static readonly HashSet<string> StudentRoleOverrides =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "ali.anani@aub.edu.lb",
            "aa624@aub.edu.lb",
        };

    public static void EnsureCanManage(UserRole role, string email)
    {
        if (role is UserRole.Student or UserRole.Admin)
        {
            return;
        }

        if (StudentRoleOverrides.Contains(email))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only students can manage a student profile.");
    }
}
