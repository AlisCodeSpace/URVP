namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // NOTIFICATION ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Notification types for different events
    /// </summary>
    public enum NotificationType : byte
    {
        ChallengeSubmitted = 0,
        ChallengeApproved = 1,
        ChallengeRejected = 2,
        FacultySpecialistInvited = 3,
        FacultySpecialistResponded = 4,
        ChallengeMatched = 5,
        PartnerRegistered = 6,
        PartnerApproved = 7,
        PartnerRejected = 8,
        ThemeSubmitted = 9,
        ThemeApproved = 10,
        ThemeRejected = 11,
        ResearchFieldSubmitted = 12,
        ResearchFieldApproved = 13,
        ResearchFieldRejected = 14,
        PartnerCriticalUpdate = 15,
        ChallengeEditRequested = 16,
        ChallengeEditRequestApproved = 17,
        ChallengeEditRequestRejected = 18,
        RDProjectSubmitted = 19,
        RDProjectApproved = 20,
        RDProjectRejected = 21,
        RDProjectFacultySpecialistInvited = 22,
        RDProjectFacultySpecialistResponded = 23,
        RDProjectMatched = 24,
        RDProjectEditRequested = 25,
        RDProjectEditRequestApproved = 26,
        RDProjectEditRequestRejected = 27
    }

    /// <summary>
    /// Notification priority levels
    /// </summary>
    public enum NotificationPriority : byte
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
