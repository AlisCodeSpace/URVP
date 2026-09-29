namespace FEA.URVP.Domain.Enums;

public enum ProjectStatus : byte
{
    Open = 0,
    Matching = 1,
    Closed = 2,

    /// <summary>
    /// The project's academic cycle has ended. Hidden from the catalog until
    /// the posting faculty reactivates it for a new cycle.
    /// </summary>
    Inactive = 3
}
