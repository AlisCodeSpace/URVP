namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // R&D PROJECT ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// RDProject.Status (mirrors ChallengeStatus)
    /// </summary>
    public enum RDProjectStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Matched = 3
    }

    /// <summary>
    /// RDProject.MatchingStatus (mirrors ChallengeMatchingStatus)
    /// </summary>
    public enum RDProjectMatchingStatus : byte
    {
        NoInvite = 0,
        Pending = 1,
        AwaitingApproval = 2,
        Complete = 3
    }

    /// <summary>
    /// RDProjectMatchInvite.Status
    /// </summary>
    public enum RDProjectInviteStatus : byte
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    /// <summary>
    /// RDProjectEditRequest.Status
    /// </summary>
    public enum RDProjectEditRequestStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
