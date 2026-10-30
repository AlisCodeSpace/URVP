using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Notifications;

/// <summary>
/// Single source of in-app and email copy. Handlers must not hardcode titles or messages.
/// </summary>
public static class NotificationMessages
{
    public static class Projects
    {
        public static string ProjectApprovedTitle() => "Project approved";

        public static string ProjectApprovedMessage(string projectTitle) =>
            $"Your project \"{projectTitle}\" has been approved and is visible to students.";

        public static string ProjectOpenTitle() => "New project available";

        public static string ProjectOpenMessage(string projectTitle) =>
            $"A new research project is open: \"{projectTitle}\".";

        public static string ProjectClosedTitle() => "Project closed";

        public static string ProjectClosedMessage(string projectTitle) =>
            $"The project \"{projectTitle}\" is now closed.";

        public static string ProjectDeletedTitle() => "Project removed";

        public static string ProjectDeletedMessage(string projectTitle) =>
            $"Your project \"{projectTitle}\" was removed by an administrator.";
    }

    public static class Placements
    {
        public static string PlacementConfirmedTitle() => "Placement confirmed";

        public static string PlacementConfirmedMessage(string projectTitle) =>
            $"You have been placed on \"{projectTitle}\".";

        public static string PlacementDeclinedTitle() => "Placement declined";

        public static string PlacementDeclinedMessage(string projectTitle) =>
            $"The placement on \"{projectTitle}\" was declined.";

        public static string PlacementCancelledTitle() => "Placement cancelled";

        public static string PlacementCancelledMessage(string projectTitle) =>
            $"The placement on \"{projectTitle}\" was cancelled.";
    }

    public static class Matching
    {
        public static string MatchingConfirmedTitle() => "Matching results published";

        public static string MatchingConfirmedMessage() =>
            "Matching results have been published. Check your placement status.";
    }

    public static class Rankings
    {
        public static string RankingSubmittedTitle() => "Project ranking received";

        public static string RankingSubmittedMessage(string studentName, string projectTitle) =>
            $"{studentName} ranked your project \"{projectTitle}\".";

        public static string RankingRemovedTitle() => "Project ranking withdrawn";

        public static string RankingRemovedMessage(string studentName, string projectTitle) =>
            $"{studentName} withdrew their ranking for \"{projectTitle}\".";

        public static string FacultyRankingSubmittedTitle() => "Candidate ranking updated";

        public static string FacultyRankingSubmittedMessage(string projectTitle) =>
            $"Faculty rankings were updated for \"{projectTitle}\".";
    }

    public static class Semesters
    {
        public static string ApplicationWindowOpenedTitle() => "Application window open";

        public static string ApplicationWindowOpenedMessage(string semesterName) =>
            $"The application window for {semesterName} is now open.";

        public static string ApplicationWindowClosedTitle() => "Application window closed";

        public static string ApplicationWindowClosedMessage(string semesterName) =>
            $"The application window for {semesterName} is now closed.";

        public static string SemesterCycleStartedTitle() => "Program cycle started";

        public static string SemesterCycleStartedMessage(string semesterName) =>
            $"The {semesterName} program cycle has started.";
    }

    public static class Profiles
    {
        public static string StudentProfileSubmittedTitle() => "Student profile submitted";

        public static string StudentProfileSubmittedMessage(string studentName) =>
            $"{studentName} submitted a student profile.";
    }

    public static class News
    {
        public static string NewsPublishedTitle() => "New announcement";

        public static string NewsPublishedMessage(string articleTitle) =>
            $"A new announcement was published: \"{articleTitle}\".";
    }

    public static class Workshops
    {
        public static string WorkshopAnnouncedTitle() => "Workshop announced";

        public static string WorkshopAnnouncedMessage(string workshopTitle) =>
            $"A new workshop is available: \"{workshopTitle}\".";
    }

    public static class Users
    {
        public static string RoleAssignedTitle() => "Role updated";

        public static string RoleAssignedMessage(string roleName) =>
            $"Your URVP role is now {roleName}.";
    }

    /// <summary>
    /// Role-assignment emails include a portal sign-in CTA. There is no invite workflow.
    /// </summary>
    public static bool RequiresSignInAction(string type) =>
        type.Equals("Invitation", StringComparison.OrdinalIgnoreCase)
        || type.Equals(nameof(NotificationType.RoleAssigned), StringComparison.OrdinalIgnoreCase);

    public static bool RequiresSignInAction(NotificationType type) =>
        RequiresSignInAction(type.ToString());
}
