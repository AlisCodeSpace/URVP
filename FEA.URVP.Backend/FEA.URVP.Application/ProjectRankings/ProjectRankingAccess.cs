using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.ProjectRankings;

internal static class ProjectRankingAccess
{
    /// <summary>Temporary FE testing override — keep aligned with StudentProfileAccess.</summary>
    private static readonly HashSet<string> StudentRoleOverrides =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "ali.anani@aub.edu.lb",
            "aa624@aub.edu.lb",
        };

    public static void EnsureCanRank(UserRole role, string email)
    {
        if (role is UserRole.Student or UserRole.Admin)
        {
            return;
        }

        if (StudentRoleOverrides.Contains(email))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only students can rank projects.");
    }
}
