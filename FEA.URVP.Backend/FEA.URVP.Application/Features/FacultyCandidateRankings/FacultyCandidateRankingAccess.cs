using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.FacultyCandidateRankings;

internal static class FacultyCandidateRankingAccess
{
    public static void EnsureCanRank(UserRole role)
    {
        if (role is UserRole.Faculty or UserRole.Admin)
        {
            return;
        }

        throw new UnauthorizedAccessException("Only faculty or admins can rank student candidates.");
    }
}
