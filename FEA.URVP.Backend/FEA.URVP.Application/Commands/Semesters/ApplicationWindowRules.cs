using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Commands.Semesters;

internal static class ApplicationWindowRules
{
    public const string RankingsClosedMessage =
        "The student application window is closed. Rankings cannot be changed.";

    public const string MatchingWhileOpenMessage =
        "Close the student application window before matching or assigning students.";

    public static void EnsureOpenForRanking(Semester? semester, DateTime utcNow)
    {
        if (semester is null || !semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(RankingsClosedMessage);
        }
    }

    public static void EnsureClosedForMatching(Semester semester, DateTime utcNow)
    {
        if (semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(MatchingWhileOpenMessage);
        }
    }
}
