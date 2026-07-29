namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // CHALLENGE ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Challenge.Status (extends ApprovalStatus with Matched)
    /// </summary>
    public enum ChallengeStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Matched = 3
    }

    /// <summary>
    /// Challenge.MatchingStatus
    /// </summary>
    public enum ChallengeMatchingStatus : byte
    {
        NoInvite = 0,
        Pending = 1,
        AwaitingApproval = 2,
        Complete = 3
    }

    /// <summary>
    /// ChallengeParticipation.Status (alias for ResponseStatus)
    /// </summary>
    public enum ParticipationStatus : byte
    {
        Applied = 0,
        Accepted = 1,
        Rejected = 2
    }

    /// <summary>
    /// ChallengeMatchInvite.Status (alias for ResponseStatus)
    /// </summary>
    public enum InviteStatus : byte
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    /// <summary>
    /// ChallengeEditRequest.Status
    /// </summary>
    public enum EditRequestStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
