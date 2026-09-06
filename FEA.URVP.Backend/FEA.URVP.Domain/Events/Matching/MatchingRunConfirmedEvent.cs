namespace FEA.URVP.Domain.Events.Matching;

public sealed record MatchingRunConfirmedPlacement(
    Guid PlacementId,
    Guid StudentUserId,
    Guid ProjectId,
    string ProjectTitle);

public sealed class MatchingRunConfirmedEvent : FEA.URVP.Domain.Events.DomainEvent
{
    public MatchingRunConfirmedEvent(
        Guid runId,
        Guid confirmedByUserId,
        IReadOnlyList<MatchingRunConfirmedPlacement> placements)
    {
        RunId = runId;
        ConfirmedByUserId = confirmedByUserId;
        Placements = placements;
    }

    public Guid RunId { get; }
    public Guid ConfirmedByUserId { get; }
    public IReadOnlyList<MatchingRunConfirmedPlacement> Placements { get; }
}
