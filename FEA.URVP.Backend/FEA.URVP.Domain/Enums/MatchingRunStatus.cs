namespace FEA.URVP.Domain.Enums;

public enum MatchingRunStatus : byte
{
    /// <summary>Proposed placements awaiting admin review.</summary>
    Draft = 0,

    /// <summary>Placements published; seats are counted as filled.</summary>
    Confirmed = 1,

    /// <summary>Superseded or rejected by an admin; placements are void.</summary>
    Discarded = 2
}
