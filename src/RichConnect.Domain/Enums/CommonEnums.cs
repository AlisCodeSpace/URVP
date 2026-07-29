namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // COMMON STATUS ENUMS (Consolidated)
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Common approval status used by CommunityPartner, Theme, and Challenge
    /// </summary>
    public enum ApprovalStatus : byte
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    /// <summary>
    /// Common response status used by Participation and Invites
    /// </summary>
    public enum ResponseStatus : byte
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    /// <summary>
    /// Creator type for research fields
    /// </summary>
    public enum CreatorType : byte
    {
        Admin = 0,
        Faculty = 1
    }
}
