using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Commands.Semesters;

internal static class ApplicationWindowRules
{
    public const string RankingsClosedMessage =
        "The student application window is closed. Rankings cannot be changed.";

    public const string MatchingWhileOpenMessage =
        "Close the student application window before running matching.";

    public const string AssignmentWhileOpenMessage =
        "Close the student application window before assigning students.";

    public const string ProfilesClosedMessage =
        "Student profiles can only be created or updated while the registration window is open.";

    public const string ProjectsClosedMessage =
        "Projects can only be posted or edited while the registration window is open.";

    public const string StudentsCannotViewProjectsMessage =
        "Projects are visible to students only while the application window is open.";

    public const string MatchingWhileRegistrationOpenMessage =
        "Close the registration window before running matching.";

    public const string AssignmentWhileRegistrationOpenMessage =
        "Close the registration window before assigning students.";

    public static void EnsureOpenForRanking(Semester? semester, DateTime utcNow)
    {
        if (semester is null || !semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(RankingsClosedMessage);
        }
    }

    public static void EnsureRegistrationOpen(Semester? semester, DateTime utcNow, string closedMessage)
    {
        if (semester is null || !semester.IsRegistrationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(closedMessage);
        }
    }

    public static void EnsureStudentsCanViewProjects(Semester? semester, DateTime utcNow, bool viewerIsStudent)
    {
        if (!viewerIsStudent)
            return;

        if (semester is null || !semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(StudentsCannotViewProjectsMessage);
        }
    }

    public static void EnsureClosedForMatching(Semester semester, DateTime utcNow)
    {
        if (semester.IsRegistrationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(MatchingWhileRegistrationOpenMessage);
        }

        if (semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(MatchingWhileOpenMessage);
        }
    }

    public static void EnsureClosedForAssignment(Semester semester, DateTime utcNow)
    {
        if (semester.IsRegistrationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(AssignmentWhileRegistrationOpenMessage);
        }

        if (semester.IsApplicationWindowOpen(utcNow))
        {
            throw new InvalidOperationException(AssignmentWhileOpenMessage);
        }
    }
}
