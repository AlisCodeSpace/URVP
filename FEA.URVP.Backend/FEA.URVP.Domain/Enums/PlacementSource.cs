namespace FEA.URVP.Domain.Enums;

/// <summary>How a placement was created.</summary>
public enum PlacementSource : byte
{
    /// <summary>Produced by the automatic matching algorithm.</summary>
    Algorithm = 0,

    /// <summary>Assigned directly by an administrator.</summary>
    Manual = 1
}
