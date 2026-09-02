namespace FEA.URVP.Domain.Enums;

public enum PlacementStatus : byte
{
    /// <summary>Produced by a draft run; not yet binding.</summary>
    Proposed = 0,

    /// <summary>Confirmed by an admin; occupies a project seat.</summary>
    Confirmed = 1,

    /// <summary>Student declined the placement; the seat is released.</summary>
    Declined = 2,

    /// <summary>Withdrawn by an admin or voided with its run; the seat is released.</summary>
    Cancelled = 3
}
