namespace FEA.URVP.Domain.Enums;

/// <summary>
/// In-app notification kinds. Stored and sent to the frontend as <see cref="Enum.ToString"/>.
/// Unused values remain for compatibility; do not invent product workflows to match them.
/// </summary>
public enum NotificationType
{
    ProjectApproved,
    ProjectOpen,
    ProjectClosed,
    ProjectDeleted,
    PlacementConfirmed,
    PlacementDeclined,
    PlacementCancelled,
    MatchingConfirmed,
    RankingSubmitted,
    RankingRemoved,
    FacultyRankingSubmitted,
    ApplicationWindowOpened,
    ApplicationWindowClosed,
    SemesterCycleStarted,
    StudentProfileSubmitted,
    NewsPublished,
    WorkshopAnnounced,
    RoleAssigned
}
