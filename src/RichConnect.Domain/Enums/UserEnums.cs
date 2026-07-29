namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // USER & AUTHENTICATION ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// User.Role (replaces magic strings like "Admin", "Partner", etc.)
    /// </summary>
    public enum UserRole : byte
    {
        Admin = 0,
        CommunityPartner = 1,
        RichTeam = 2,
        FacultySpecialist = 3
    }
}
